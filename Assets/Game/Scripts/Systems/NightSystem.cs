using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Ночь. Счёт сначала показывается, и только когда игрок ложится спать —
/// применяется. Из-за этого ползунки успевают спасти забег, а смерть от голода
/// читается как своя ошибка, а не как подстава.
/// </summary>
public class NightSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<PhaseChangedEvent> _phaseStarts;
    private EcsFilter<PhaseEndedEvent> _phaseEnds;
    private EcsFilter<SetTaxEvent> _taxChanges;

    private EcsFilter<RunFlag, TaxAttribute, TreasuryAttribute> _runs;
    private EcsFilter<LordFlag, OpinionAttribute, TraitsAttribute>.Exclude<LeftCourtFlag> _lords;
    private EcsFilter<PlayerFlag, TraitsAttribute> _players;

    public void Init()
    {
        if (UI.Night.PeasantSlider != null)
            UI.Night.PeasantSlider.onValueChanged.AddListener(OnPeasantTax);
        if (UI.Night.LordSlider != null)
            UI.Night.LordSlider.onValueChanged.AddListener(OnLordTax);
    }

    public void Destroy()
    {
        if (UI.Night.PeasantSlider != null)
            UI.Night.PeasantSlider.onValueChanged.RemoveListener(OnPeasantTax);
        if (UI.Night.LordSlider != null)
            UI.Night.LordSlider.onValueChanged.RemoveListener(OnLordTax);
    }

    public void Run()
    {
        foreach (var i in _phaseStarts)
            if (_phaseStarts.Get1(i).Phase == DayPhase.Night) EnterNight();

        foreach (var i in _taxChanges)
        {
            ApplySetting(_taxChanges.Get1(i));
            Refresh();
        }

        foreach (var i in _phaseEnds)
            if (_phaseEnds.Get1(i).Phase == DayPhase.Night) Settle();
    }

    // ─────────── показ ───────────

    private void EnterNight()
    {
        foreach (var r in _runs)
        {
            ref var tax = ref _runs.Get2(r);
            UI.Night.SetSliders(tax.Peasants, tax.Lords);
        }

        Refresh();
    }

    private void Refresh()
    {
        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var treasury = ref _runs.Get3(r);

            var report = Calculate(r);
            entity.Get<NightReportAttribute>() = report;

            int goldAfter = Mathf.Max(0, treasury.Gold + report.GoldNet);
            int foodAfter = Mathf.Max(0, treasury.Food + report.FoodNet);

            UI.Night.SetReport(report, goldAfter, foodAfter, Warning(report, foodAfter));
        }
    }

    private string Warning(NightReportAttribute report, int foodAfter)
    {
        if (report.Starving) return "Амбары пусты. Гарнизон ляжет спать голодным.";
        if (foodAfter <= report.FoodUpkeep) return "Пищи хватит на одну ночь. Не больше.";
        if (report.MemoryPenalty <= -6) return "Крестьяне помнят прошлые поборы. И считают.";
        if (report.CommonsOpinionDelta <= -15) return "Крестьяне точат вилы.";
        return string.Empty;
    }

    // ─────────── счёт ───────────

    private NightReportAttribute Calculate(int runIndex)
    {
        var balance = GameConfig.BalanceConfig;
        var chars = GameConfig.CharactersConfig;

        ref var tax = ref _runs.Get2(runIndex);
        ref var treasury = ref _runs.Get3(runIndex);

        int grudge = _runs.GetEntity(runIndex).Get<CommonsMemoryAttribute>().Grudge;

        var report = new NightReportAttribute
        {
            GoldIncome = balance.LordGold(tax.Lords) * LordsAtCourt() / Mathf.Max(1, balance.LordsCount),
            FoodIncome = balance.PeasantFood(tax.Peasants),
            FoodUpkeep = balance.FoodUpkeep(treasury.Garrison),
            LordOpinionDelta = balance.LordOpinion(tax.Lords),
            MemoryPenalty = -grudge * balance.GrudgePerLevel,
        };

        // Сегодняшнее недовольство плюс всё, что накопилось за прошлые ночи.
        report.CommonsOpinionDelta = balance.PeasantOpinion(tax.Peasants) + report.MemoryPenalty;

        foreach (var p in _players)
        {
            ref var traits = ref _players.Get2(p);
            AddPassive(ref report, chars.GetTrait(traits.A));
            AddPassive(ref report, chars.GetTrait(traits.B));
        }

        report.Starving = treasury.Food + report.FoodIncome - report.FoodUpkeep < 0;
        return report;
    }

    private static void AddPassive(ref NightReportAttribute report, TraitDefinition trait)
    {
        if (trait == null) return;
        report.GoldIncome += trait.GoldPerDay;
        report.FoodIncome += trait.FoodPerDay;
    }

    private int LordsAtCourt()
    {
        int count = 0;
        foreach (var _ in _lords) count++;
        return count;
    }

    // ─────────── применение ───────────

    private void Settle()
    {
        var balance = GameConfig.BalanceConfig;
        var chars = GameConfig.CharactersConfig;

        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var treasury = ref _runs.Get3(r);
            var report = entity.Get<NightReportAttribute>();

            treasury.Gold = Mathf.Max(0, treasury.Gold + report.GoldNet);
            treasury.Food = treasury.Food + report.FoodNet;

            ref var starving = ref entity.Get<StarvingAttribute>();

            if (treasury.Food < 0)
            {
                treasury.Food = 0;
                entity.Get<StarvingFlag>();
                starving.Nights++;
            }
            else
            {
                if (entity.Has<StarvingFlag>()) entity.Del<StarvingFlag>();
                starving.Nights = 0;
            }

            ref var memory = ref entity.Get<CommonsMemoryAttribute>();
            ref var tax = ref _runs.Get2(r);

            int over = Mathf.Max(0, tax.Peasants - balance.TaxNeutralLevel);
            memory.Grudge = over > 0
                ? Mathf.Min(balance.GrudgeMax, memory.Grudge + over)
                : Mathf.Max(0, memory.Grudge - balance.GrudgeDecay);

            ref var commons = ref entity.Get<CommonsAttribute>();
            commons.Opinion = balance.ClampOpinion(commons.Opinion + report.CommonsOpinionDelta
                + (entity.Has<StarvingFlag>() ? balance.StarvingCommonsPenalty : 0));

            foreach (var l in _lords)
            {
                ref var opinion = ref _lords.Get2(l);
                ref var traits = ref _lords.Get3(l);

                int drift = report.LordOpinionDelta
                    + Drift(chars.GetTrait(traits.A))
                    + Drift(chars.GetTrait(traits.B));

                opinion.Value = balance.ClampOpinion(opinion.Value + drift);
            }
        }
    }

    private static int Drift(TraitDefinition trait) => trait != null ? trait.OpinionDriftPerDay : 0;

    // ─────────── ползунки ───────────

    private void OnPeasantTax(float value) => Raise(TaxKind.Peasants, value);
    private void OnLordTax(float value) => Raise(TaxKind.Lords, value);

    private void Raise(TaxKind kind, float value)
    {
        ref var request = ref _world.NewEntity().Get<SetTaxEvent>();
        request.Kind = kind;
        request.Value = Mathf.RoundToInt(value);
    }

    private void ApplySetting(SetTaxEvent request)
    {
        foreach (var r in _runs)
        {
            ref var tax = ref _runs.Get2(r);
            if (request.Kind == TaxKind.Peasants) tax.Peasants = request.Value;
            else tax.Lords = request.Value;
        }
    }
}
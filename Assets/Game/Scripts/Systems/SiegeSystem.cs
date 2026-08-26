using System.Text;
using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Осада. Считает силы один раз на рассвете последнего дня и дальше только следит,
/// как они тают от стычек.
///
/// Лорды приезжают КОМАНДИРАМИ, без войск: копья, которые ты у них выпрашивал,
/// давно стоят в твоём гарнизоне и всю игру ели твою пищу, а остальные остались
/// защищать их собственные замки. Поэтому пять лордов — это не пять армий,
/// а пятеро, кто поведёт твои отряды, и разница огромна.
/// </summary>
public class SiegeSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<PhaseEndedEvent> _phaseEnds;
    private EcsFilter<RunFlag, SiegeAttribute, TreasuryAttribute, CommonsAttribute>.Exclude<RunOverFlag> _runs;
    private EcsFilter<LordFlag, OpinionAttribute, PersonAttribute>.Exclude<DeadFlag, LeftCourtFlag> _lords;
    private EcsFilter<BuildingAttribute> _buildings;

    private readonly StringBuilder _text = new StringBuilder();

    public void Run()
    {
        var balance = GameConfig.BalanceConfig;

        foreach (var i in _phaseEnds)
        {
            ref var ended = ref _phaseEnds.Get1(i);
            if (ended.Phase != DayPhase.Night) continue;
            if (ended.Day < balance.DaysUntilSiege) continue;

            Begin(balance);
        }

        Watch();
    }

    // ─────────────────────── начало ───────────────────────

    private void Begin(BalanceConfig balance)
    {
        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var siege = ref _runs.Get2(r);
            if (siege.Running) continue;

            ref var treasury = ref _runs.Get3(r);
            int commons = _runs.Get4(r).Opinion;

            int lords = RallyCommanders(balance);
            int morale = Morale(balance, lords, treasury.Food, commons, out string summary);

            siege.Running = true;
            siege.Morale = morale;
            siege.LordsPresent = lords;
            siege.OurStart = siege.OurForce = Mathf.Max(1, treasury.Garrison);
            siege.EnemyStart = siege.EnemyForce = balance.SiegeEnemyForce;
            siege.Summary = summary;
            siege.NextSpawn = Time.time;

            siege.EnemyRoutAt = siege.EnemyStart * balance.EnemyRoutPercent / 100;
            siege.OurRoutAt = Mathf.Max(0,
                siege.OurStart * balance.OurRoutPercent / 100 - morale / Mathf.Max(1, balance.MoraleToRoutRelief));

            entity.Get<PhaseLockFlag>();

            _world.NewEntity().Get<SiegeStartedEvent>();
            _world.NewEntity().Get<ChangeScreenEvent>().Target = ScreenId.Siege;
            _world.NewEntity().Get<ChronicleEvent>().Line =
                $"Они пришли на рассвете. Нас {siege.OurForce}, их {siege.EnemyForce}.";
        }
    }

    /// <summary>Кто приехал командовать. Мнение решает, любовнику хватает меньшего.</summary>
    private int RallyCommanders(BalanceConfig balance)
    {
        int count = 0;

        foreach (var i in _lords)
        {
            var lord = _lords.GetEntity(i);
            int threshold = lord.Has<LoverFlag>() ? balance.LoverComeAtOpinion : balance.TroopsComeAtOpinion;
            if (_lords.Get2(i).Value < threshold) continue;

            lord.Get<CameToSiegeFlag>();
            count++;
        }

        return count;
    }

    private int Morale(BalanceConfig balance, int lords, int food, int commons, out string summary)
    {
        _text.Length = 0;

        int fromLords = balance.LordCountBonus.Length > 0
            ? balance.LordCountBonus[Mathf.Clamp(lords, 0, balance.LordCountBonus.Length - 1)]
            : 0;

        int fromFood = food <= 0
            ? balance.FoodPenaltyMax
            : Mathf.Clamp(food * balance.FoodBonusMax / Mathf.Max(1, balance.FoodComfort),
                balance.FoodPenaltyMax, balance.FoodBonusMax);

        int fromWalls = Walls();
        int fromCommons = commons / Mathf.Max(1, balance.CommonsDivider);

        Line("Командиров", lords, fromLords);
        Line("Припасы", food, fromFood);
        Line("Стены", fromWalls, fromWalls);
        Line("Крестьяне", commons, fromCommons);

        summary = _text.ToString();
        return fromLords + fromFood + fromWalls + fromCommons;
    }

    private void Line(string name, int value, int bonus)
    {
        if (_text.Length > 0) _text.Append("   ");
        _text.Append(name).Append(' ').Append(value)
             .Append(" (").Append(bonus > 0 ? "+" : string.Empty).Append(bonus).Append(')');
    }

    private int Walls()
    {
        int total = 0;

        foreach (var b in _buildings)
        {
            ref var building = ref _buildings.Get1(b);
            if (building.Level <= 0) continue;

            var definition = GameConfig.BuildingsConfig.GetBuilding(building.Id);
            var tier = definition != null ? definition.Tier(building.Level) : null;
            if (tier != null) total += tier.SiegeDefence;
        }

        return total;
    }

    // ─────────────────────── ход осады ───────────────────────

    private void Watch()
    {
        foreach (var r in _runs)
        {
            ref var siege = ref _runs.Get2(r);
            if (!siege.Running) continue;

            UI.Siege.SetForces(siege.OurForce, siege.EnemyForce);
            UI.Siege.SetSummary(siege.Summary);

            if (siege.EnemyForce <= siege.EnemyRoutAt)
            {
                Finish(ref siege, true);
                continue;
            }

            if (siege.OurForce <= siege.OurRoutAt) Finish(ref siege, false);
        }
    }

    private void Finish(ref SiegeAttribute siege, bool won)
    {
        siege.Running = false;

        if (won)
        {
            _world.NewEntity().Get<VictoryEvent>().Defence = siege.OurForce;
            return;
        }

        ref var death = ref _world.NewEntity().Get<DeathEvent>();
        death.Cause = DeathCause.Siege;
        death.KillerLordId = -1;
        death.Detail = string.Empty;
    }
}
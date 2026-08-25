using Leopotam.Ecs;

/// <summary>
/// Постройки. Владеет уровнями, стройкой и карточкой: ничего из этого больше
/// никто не трогает, поэтому логика и рисование живут вместе без риска разойтись.
///
/// Стройка занимает дни: первый уровень встаёт к утру, второй за сутки, третий за двое.
/// Числа в BuildingsConfig, здесь только правило «готово, когда день дорос до ReadyOnDay».
/// </summary>
public class BuildingSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private const int NoTarget = -1;

    private EcsWorld _world;

    private EcsFilter<BuildingPinClickedEvent> _clicks;
    private EcsFilter<BuildRequestEvent> _requests;
    private EcsFilter<CloseCastleCardEvent> _closes;
    private EcsFilter<PhaseChangedEvent> _phaseStarts;

    private EcsFilter<RunFlag, CalendarAttribute, TreasuryAttribute> _runs;
    private EcsFilter<BuildingAttribute> _buildings;

    private int _shown = NoTarget;

    public void Init() => Subscribe(true);
    public void Destroy() => Subscribe(false);

    public void Run()
    {
        foreach (var i in _phaseStarts)
            if (_phaseStarts.Get1(i).Phase == DayPhase.Morning) FinishBuilds();

        foreach (var i in _clicks) Open(_clicks.Get1(i).Id);
        foreach (var _ in _closes) Close();
        foreach (var i in _requests) Build(_requests.Get1(i).Id, _requests.Get1(i).Level);

        RefreshPins();
        RefreshCard();
    }

    // ─────────────────────── стройка ───────────────────────

    private void Build(BuildingId id, int level)
    {
        var config = GameConfig.BuildingsConfig;
        var definition = config.GetBuilding(id);
        if (definition == null) return;

        var tier = definition.Tier(level);
        if (tier == null) return;

        foreach (var r in _runs)
        {
            ref var calendar = ref _runs.Get2(r);
            ref var treasury = ref _runs.Get3(r);

            if (calendar.Phase != DayPhase.Day || calendar.ActionsLeft <= 0) return;
            if (!TryFind(id, out int index)) return;

            ref var building = ref _buildings.Get1(index);
            if (building.IsBuilding || building.Level + 1 != level) return;
            if (!tier.CanAfford(treasury.Gold, treasury.Food)) return;

            treasury.Gold -= tier.GoldCost;
            treasury.Food -= tier.FoodCost;
            calendar.ActionsLeft--;

            building.TargetLevel = level;
            building.ReadyOnDay = calendar.Day + tier.BuildDays;

            Chronicle(tier.BuildDays > 0
                ? $"Заложили: {tier.Title}. Работы на {tier.BuildDays} дн."
                : $"Заложили: {tier.Title}. К утру встанет.");

            if (tier.BuildDays <= 0) return;   // достроится на рассвете, как и остальные
        }
    }

    private void FinishBuilds()
    {
        var config = GameConfig.BuildingsConfig;

        foreach (var r in _runs)
        {
            int day = _runs.Get2(r).Day;

            foreach (var b in _buildings)
            {
                ref var building = ref _buildings.Get1(b);
                if (!building.IsBuilding || day < building.ReadyOnDay) continue;

                building.Level = building.TargetLevel;

                var definition = config.GetBuilding(building.Id);
                var tier = definition != null ? definition.Tier(building.Level) : null;
                Chronicle(tier != null ? $"Готово: {tier.Title}." : "Стройка окончена.");
            }
        }
    }

    // ─────────────────────── экран ───────────────────────

    private void Open(BuildingId id)
    {
        _shown = (int)id;
        UI.CastleActions.SetVisible(false);
    }

    private void Close()
    {
        _shown = NoTarget;
        UI.BuildingCard.SetVisible(false);
    }

    private void RefreshPins()
    {
        var config = GameConfig.BuildingsConfig;

        foreach (var b in _buildings)
        {
            var actor = _buildings.GetEntity(b).Get<BuildingRef>().Value;
            if (actor == null) continue;

            ref var building = ref _buildings.Get1(b);
            var definition = config.GetBuilding(building.Id);
            if (definition == null) continue;

            actor.SetLabel(definition.TitleAt(building.Level),
                building.IsBuilding ? "строится" :
                building.Level > 0 ? building.Level.ToString() : string.Empty);
        }
    }

    private void RefreshCard()
    {
        if (_shown == NoTarget) return;

        var config = GameConfig.BuildingsConfig;
        var id = (BuildingId)_shown;
        var definition = config.GetBuilding(id);

        if (definition == null || !TryFind(id, out int index))
        {
            Close();
            return;
        }

        ref var building = ref _buildings.Get1(index);
        var tier = definition.Tier(building.Level);

        foreach (var r in _runs)
        {
            ref var calendar = ref _runs.Get2(r);
            ref var treasury = ref _runs.Get3(r);

            string progress = building.IsBuilding
                ? $"Строится: {definition.TitleAt(building.TargetLevel)} — осталось {Days(building.ReadyOnDay - calendar.Day)}"
                : string.Empty;

            UI.BuildingCard.Show(
                definition.TitleAt(building.Level),
                building.Level,
                definition.MaxLevel,
                definition.Description,
                tier != null ? tier.BonusLine(GameConfig.CharactersConfig) : string.Empty,
                progress);

            UI.BuildingCard.SetTiers(definition, building.Level, building.IsBuilding,
                calendar.Phase == DayPhase.Day && calendar.ActionsLeft > 0 ? treasury.Gold : -1,
                treasury.Food);
        }
    }

    private static string Days(int value) =>
        value <= 0 ? "до утра" : value == 1 ? "один день" : value + " дн.";

    // ─────────────────────── мелочи ───────────────────────

    private bool TryFind(BuildingId id, out int index)
    {
        foreach (var b in _buildings)
        {
            if (_buildings.Get1(b).Id != id) continue;
            index = b;
            return true;
        }

        index = -1;
        return false;
    }

    private void Subscribe(bool on)
    {
        if (UI.BuildingCard.CloseButton != null)
        {
            if (on) UI.BuildingCard.CloseButton.onClick.AddListener(RequestClose);
            else UI.BuildingCard.CloseButton.onClick.RemoveListener(RequestClose);
        }

        var tiers = UI.BuildingCard.Tiers;
        if (tiers == null) return;

        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i] == null) continue;

            if (on)
            {
                int level = i + 1;   // локальная копия, иначе все кнопки строят третий уровень
                tiers[i].onClick.AddListener(() => RequestBuild(level));
            }
            else
            {
                tiers[i].onClick.RemoveAllListeners();
            }
        }
    }

    private void RequestBuild(int level)
    {
        if (_shown == NoTarget) return;

        ref var request = ref _world.NewEntity().Get<BuildRequestEvent>();
        request.Id = (BuildingId)_shown;
        request.Level = level;
    }

    private void RequestClose() => _world.NewEntity().Get<CloseCastleCardEvent>();

    private void Chronicle(string line) => _world.NewEntity().Get<ChronicleEvent>().Line = line;
}
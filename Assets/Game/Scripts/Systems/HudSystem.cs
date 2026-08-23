using Leopotam.Ecs;

/// <summary>
/// Верхняя полоса. Тексты переписываются только когда цифра изменилась —
/// иначе TMP перестраивает меш каждый кадр на пустом месте.
/// </summary>
public class HudSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<CourtReadyEvent> _courtReady;
    private EcsFilter<RunReadyEvent> _runReady;
    private EcsFilter<RunFlag, CalendarAttribute, TreasuryAttribute> _runs;
    private EcsFilter<RunFlag, RunOverFlag> _finished;
    private EcsFilter<RunFlag, PhaseLockFlag> _locked;

    private int _day = int.MinValue;
    private int _phase = int.MinValue;
    private int _actions = int.MinValue;
    private int _gold = int.MinValue;
    private int _food = int.MinValue;
    private int _garrison = int.MinValue;

    public void Init()
    {
        if (UI.Hud.NextPhaseButton != null)
            UI.Hud.NextPhaseButton.onClick.AddListener(RequestAdvance);
    }

    public void Destroy()
    {
        if (UI.Hud.NextPhaseButton != null)
            UI.Hud.NextPhaseButton.onClick.RemoveListener(RequestAdvance);
    }

    public void Run()
    {
        foreach (var _ in _courtReady)
        {
            UI.Hud.SetVisible(false);
            Invalidate();
        }

        foreach (var _ in _runReady) UI.Hud.SetVisible(true);

        foreach (var r in _runs) Refresh(r);

        if (UI.Hud.NextPhaseButton != null)
        {
            bool blocked = false;
            foreach (var _ in _finished) blocked = true;
            foreach (var _ in _locked) blocked = true;
            UI.Hud.NextPhaseButton.interactable = !blocked;
        }
    }

    private void Refresh(int runIndex)
    {
        var balance = GameConfig.BalanceConfig;
        ref var calendar = ref _runs.Get2(runIndex);
        ref var treasury = ref _runs.Get3(runIndex);

        if (_day != calendar.Day)
        {
            _day = calendar.Day;
            UI.Hud.SetDay(_day, balance.DaysUntilSiege);
        }

        if (_phase != (int)calendar.Phase)
        {
            _phase = (int)calendar.Phase;
            UI.Hud.SetPhase(calendar.Phase,
                balance.PhaseName(calendar.Phase),
                balance.PhaseButton(calendar.Phase));
        }

        if (_actions != calendar.ActionsLeft)
        {
            _actions = calendar.ActionsLeft;
            UI.Hud.SetActions(_actions);
        }

        if (_gold != treasury.Gold || _food != treasury.Food || _garrison != treasury.Garrison)
        {
            _gold = treasury.Gold;
            _food = treasury.Food;
            _garrison = treasury.Garrison;
            UI.Hud.SetResources(_gold, _food, _garrison);
        }
    }

    private void Invalidate()
    {
        _day = _phase = _actions = _gold = _food = _garrison = int.MinValue;
    }

    private void RequestAdvance() => _world.NewEntity().Get<AdvancePhaseEvent>();
}
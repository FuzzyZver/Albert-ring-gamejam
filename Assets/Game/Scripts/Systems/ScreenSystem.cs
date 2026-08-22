using Leopotam.Ecs;

/// <summary>
/// Кто и когда что видит. Фаза сама переводит на нужный экран, но между
/// утром и днём игрок волен листать карту — поэтому переход и фаза разделены.
/// Вечером и ночью навигация закрыта: ты уже всё решил.
/// </summary>
public class ScreenSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<ChangeScreenEvent> _requests;
    private EcsFilter<PhaseChangedEvent> _phaseChanges;
    private EcsFilter<CourtReadyEvent> _courtReady;
    private EcsFilter<RunReadyEvent> _runReady;
    private EcsFilter<RunFlag, CalendarAttribute, ScreenAttribute> _runs;

    public void Init()
    {
        Subscribe(true);
    }

    public void Destroy()
    {
        Subscribe(false);
    }

    public void Run()
    {
        foreach (var _ in _courtReady) Apply(ScreenId.None);
        foreach (var _ in _runReady) Apply(ScreenId.Map);

        foreach (var i in _phaseChanges) Apply(ScreenFor(_phaseChanges.Get1(i).Phase));
        foreach (var i in _requests) Apply(_requests.Get1(i).Target);

        RefreshNav();
    }

    private static ScreenId ScreenFor(DayPhase phase)
    {
        switch (phase)
        {
            case DayPhase.Morning: return ScreenId.Court;
            case DayPhase.Day: return ScreenId.Castle;
            case DayPhase.Evening: return ScreenId.Evening;
            default: return ScreenId.Night;
        }
    }

    private void Apply(ScreenId screen)
    {
        foreach (var r in _runs) _runs.Get3(r).Current = screen;

        UI.Screens.Show(screen);
        // карточку гасить не надо: SelectionSystem сбросит выделение,
        // а LordCardSystem сама спрячется. Два владельца одного окна — верный путь к морганию.
    }

    private void RefreshNav()
    {
        foreach (var r in _runs)
        {
            var phase = _runs.Get2(r).Phase;
            bool free = phase == DayPhase.Morning || phase == DayPhase.Day;

            UI.Hud.SetNavAvailable(
                map: free,
                court: phase == DayPhase.Morning,
                castle: phase == DayPhase.Day);
        }
    }

    private void Subscribe(bool on)
    {
        Bind(UI.Hud.MapButton, GoMap, on);
        Bind(UI.Hud.CourtButton, GoCourt, on);
        Bind(UI.Hud.CastleButton, GoCastle, on);
    }

    private static void Bind(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction action, bool on)
    {
        if (button == null) return;
        if (on) button.onClick.AddListener(action);
        else button.onClick.RemoveListener(action);
    }

    private void GoMap() => Request(ScreenId.Map);
    private void GoCourt() => Request(ScreenId.Court);
    private void GoCastle() => Request(ScreenId.Castle);

    private void Request(ScreenId screen) =>
        _world.NewEntity().Get<ChangeScreenEvent>().Target = screen;
}
using Leopotam.Ecs;

/// <summary>Карта видна только после выбора персонажа. Заодно перерисовывает подписи булавок.</summary>
public class MapViewSystem : Injects, IEcsRunSystem
{
    private EcsFilter<CourtReadyEvent> _courtReady;
    private EcsFilter<RunReadyEvent> _runReady;
    private EcsFilter<PersonAttribute, ActorRef> _pinned;

    public void Run()
    {
        foreach (var _ in _courtReady) SetMapVisible(false);

        foreach (var _ in _runReady)
        {
            SetMapVisible(true);
            RefreshPins();
        }
    }

    private void SetMapVisible(bool value)
    {
        if (UI.MapRoot != null) UI.MapRoot.SetActive(value);
    }

    private void RefreshPins()
    {
        foreach (var i in _pinned)
        {
            ref var person = ref _pinned.Get1(i);
            _pinned.Get2(i).Value.SetLabel(person.FullName);
        }
    }
}
using Leopotam.Ecs;

/// <summary>Подписи на булавках. Экранами заведует ScreenSystem — здесь только карта как таковая.</summary>
public class MapViewSystem : Injects, IEcsRunSystem
{
    private EcsFilter<RunReadyEvent> _runReady;
    private EcsFilter<PersonAttribute, ActorRef> _pinned;

    public void Run()
    {
        foreach (var _ in _runReady) RefreshPins();
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
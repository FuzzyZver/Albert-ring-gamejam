using Leopotam.Ecs;

/// <summary>
/// Единственный владелец выделения. Карточка и панель глаголов больше не слушают
/// клики сами — иначе они могли бы разойтись во мнении о том, кто открыт.
/// </summary>
public class SelectionSystem : Injects, IEcsRunSystem
{
    private EcsFilter<PinClickedEvent> _clicks;
    private EcsFilter<CloseCardEvent> _closes;
    private EcsFilter<ChangeScreenEvent> _screenChanges;
    private EcsFilter<PhaseChangedEvent> _phaseChanges;
    private EcsFilter<CourtReadyEvent> _newRuns;

    private EcsFilter<RunFlag, SelectionAttribute> _runs;

    public void Run()
    {
        foreach (var _ in _closes) Set(SelectionAttribute.Nobody);
        foreach (var _ in _screenChanges) Set(SelectionAttribute.Nobody);
        foreach (var _ in _phaseChanges) Set(SelectionAttribute.Nobody);
        foreach (var _ in _newRuns) Set(SelectionAttribute.Nobody);

        foreach (var i in _clicks) Select(_clicks.Get1(i).Target);
    }

    private void Select(EcsEntity target)
    {
        if (!target.IsAlive() || !target.Has<LordIdAttribute>())
        {
            Set(SelectionAttribute.Nobody);
            return;
        }

        Set(target.Get<LordIdAttribute>().Value);
    }

    private void Set(int lordId)
    {
        foreach (var r in _runs)
        {
            ref var selection = ref _runs.Get2(r);
            selection.LordId = lordId;
            _runs.GetEntity(r).Get<SelectionChangedFlag>();
        }
    }
}
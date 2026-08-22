using Leopotam.Ecs;

/// <summary>
/// Последствия. Пока применяет только цифры и пишет строку летописи —
/// смертельные варианты отмечены в конфиге, но убивать начнём на этапе 4.
/// </summary>
public class ConsequenceSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<ConsequenceEvent> _requests;
    private EcsFilter<RunFlag, CalendarAttribute, TreasuryAttribute> _runs;

    public void Run()
    {
        foreach (var i in _requests)
        {
            var request = _requests.Get1(i);
            var definition = GameConfig.CharactersConfig.GetConsequence(request.Id);
            if (definition == null) continue;

            Apply(definition, request.Source);
        }
    }

    private void Apply(ConsequenceDefinition definition, EcsEntity source)
    {
        bool sourceAlive = source.IsAlive();

        if (definition.LordOpinion != 0 && sourceAlive && source.Has<OpinionAttribute>())
        {
            ref var change = ref _world.NewEntity().Get<OpinionChangeEvent>();
            change.Target = source;
            change.Delta = definition.LordOpinion;
            change.Reason = definition.Title;
        }

        if (definition.CourtOpinion != 0)
        {
            ref var court = ref _world.NewEntity().Get<CourtOpinionChangeEvent>();
            court.Delta = definition.CourtOpinion;
            court.ExceptLordId = sourceAlive && source.Has<LordIdAttribute>()
                ? source.Get<LordIdAttribute>().Value
                : -1;
            court.Reason = definition.Title;
        }

        if (definition.CommonsOpinion != 0)
        {
            ref var commons = ref _world.NewEntity().Get<CommonsOpinionChangeEvent>();
            commons.Delta = definition.CommonsOpinion;
            commons.Reason = definition.Title;
        }

        foreach (var r in _runs)
        {
            ref var calendar = ref _runs.Get2(r);
            ref var treasury = ref _runs.Get3(r);

            treasury.Gold += definition.Gold;
            treasury.Food += definition.Food;
            treasury.Garrison += definition.Troops;

            if (definition.ActionsLost > 0)
                calendar.ActionsLeft = UnityEngine.Mathf.Max(0, calendar.ActionsLeft - definition.ActionsLost);
        }

        if (definition.IsLethalForLord && sourceAlive) source.Get<DeadFlag>();

        string line = definition.ChronicleLine;
        if (sourceAlive && source.Has<PersonAttribute>())
            line = line.Replace("{lord}", source.Get<PersonAttribute>().GivenName);

        _world.NewEntity().Get<ChronicleEvent>().Line = line;
    }
}
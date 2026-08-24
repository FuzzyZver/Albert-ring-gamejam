using Leopotam.Ecs;

/// <summary>
/// Применяет выбранный вариант. Один на всех: и утренний проситель, и вечернее
/// событие шлют ApplyChoiceEvent с одним и тем же ChoiceDefinition.
/// Мнения, как обычно, не трогает напрямую — только шлёт события.
/// </summary>
public class ChoiceEffectSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<ApplyChoiceEvent> _requests;
    private EcsFilter<RunFlag, TreasuryAttribute> _runs;
    private EcsFilter<LordFlag, LordIdAttribute> _lords;

    public void Run()
    {
        foreach (var i in _requests)
        {
            var request = _requests.Get1(i);
            if (request.Choice == null) continue;

            Apply(request.Choice, request.LordId);

            if (!string.IsNullOrEmpty(request.Result))
                _world.NewEntity().Get<ChronicleEvent>().Line = request.Result;
        }
    }

    private void Apply(ChoiceDefinition choice, int lordId)
    {
        foreach (var r in _runs)
        {
            ref var treasury = ref _runs.Get2(r);
            treasury.Gold = UnityEngine.Mathf.Max(0, treasury.Gold + choice.Gold);
            treasury.Food = UnityEngine.Mathf.Max(0, treasury.Food + choice.Food);
            treasury.Garrison = UnityEngine.Mathf.Max(0, treasury.Garrison + choice.Garrison);
        }

        if (choice.CommonsOpinion != 0)
        {
            ref var commons = ref _world.NewEntity().Get<CommonsOpinionChangeEvent>();
            commons.Delta = choice.CommonsOpinion;
            commons.Reason = choice.Label;
        }

        if (choice.CourtOpinion != 0)
        {
            ref var court = ref _world.NewEntity().Get<CourtOpinionChangeEvent>();
            court.Delta = choice.CourtOpinion;
            court.ExceptLordId = -1;
            court.Reason = choice.Label;
        }

        // Лорд нужен только для его личного мнения. Последствие летит и без лорда:
        // репа в лицо и пьяная исповедь никакого виновника не требуют.
        bool hasLord = TryFindLord(lordId, out var lord);

        if (hasLord && choice.LordOpinion != 0)
        {
            ref var opinion = ref _world.NewEntity().Get<OpinionChangeEvent>();
            opinion.Target = lord;
            opinion.Delta = choice.LordOpinion;
            opinion.Reason = choice.Label;
        }

        if (choice.Consequence == ConsequenceId.None) return;
        if (choice.RealChance < 100 && !Roll(choice.RealChance)) return;

        ref var consequence = ref _world.NewEntity().Get<ConsequenceEvent>();
        consequence.Source = hasLord ? lord : default;
        consequence.Id = choice.Consequence;
    }

    private bool Roll(int chance)
    {
        foreach (var r in _runs)
        {
            var rng = _runs.GetEntity(r).Get<RngAttribute>().Value;
            if (rng != null) return rng.Next(100) < chance;
        }

        return true;
    }

    private bool TryFindLord(int lordId, out EcsEntity found)
    {
        found = default;
        if (lordId < 0) return false;

        foreach (var i in _lords)
        {
            if (_lords.Get2(i).Value != lordId) continue;
            found = _lords.GetEntity(i);
            return true;
        }

        return false;
    }
}
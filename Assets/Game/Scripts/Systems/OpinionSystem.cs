using Leopotam.Ecs;

/// <summary>
/// Вся арифметика мнений в одном месте. Любая система, которая хочет
/// сдвинуть мнение, шлёт событие и не думает ни про потолки, ни про то,
/// кого ещё это задевает.
/// </summary>
public class OpinionSystem : Injects, IEcsRunSystem
{
    private EcsFilter<OpinionChangeEvent> _personal;
    private EcsFilter<CourtOpinionChangeEvent> _court;
    private EcsFilter<CommonsOpinionChangeEvent> _commons;

    private EcsFilter<RunFlag, CommonsAttribute> _runs;
    private EcsFilter<LordFlag, LordIdAttribute, OpinionAttribute>.Exclude<LeftCourtFlag> _lords;

    public void Run()
    {
        var balance = GameConfig.BalanceConfig;

        foreach (var i in _personal)
        {
            var change = _personal.Get1(i);
            if (!change.Target.IsAlive() || !change.Target.Has<OpinionAttribute>()) continue;

            ref var opinion = ref change.Target.Get<OpinionAttribute>();
            opinion.Value = balance.ClampOpinion(opinion.Value + change.Delta);
        }

        foreach (var i in _court)
        {
            var change = _court.Get1(i);

            foreach (var l in _lords)
            {
                if (_lords.Get2(l).Value == change.ExceptLordId) continue;

                ref var opinion = ref _lords.Get3(l);
                opinion.Value = balance.ClampOpinion(opinion.Value + change.Delta);
            }
        }

        foreach (var i in _commons)
        {
            int delta = _commons.Get1(i).Delta;

            foreach (var r in _runs)
            {
                ref var commons = ref _runs.Get2(r);
                commons.Opinion = balance.ClampOpinion(commons.Opinion + delta);
            }
        }
    }
}
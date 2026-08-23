using Leopotam.Ecs;

/// <summary>
/// Осада приходит на рассвете после ночи последнего дня. Считает всех, кто придёт:
/// гарнизон плюс копья лордов, чьё мнение выше порога и кто ещё при дворе.
/// Пока это одно сравнение — место под будущие мелкие схватки внутри осады.
/// </summary>
public class SiegeSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<PhaseEndedEvent> _phaseEnds;
    private EcsFilter<RunFlag, TreasuryAttribute>.Exclude<RunOverFlag> _runs;
    private EcsFilter<LordFlag, OpinionAttribute, TroopsAttribute>.Exclude<DeadFlag, LeftCourtFlag> _lords;

    public void Run()
    {
        var balance = GameConfig.BalanceConfig;

        foreach (var i in _phaseEnds)
        {
            ref var ended = ref _phaseEnds.Get1(i);
            if (ended.Phase != DayPhase.Night) continue;
            if (ended.Day < balance.DaysUntilSiege) continue;

            Resolve(balance);
        }
    }

    private void Resolve(BalanceConfig balance)
    {
        foreach (var r in _runs)
        {
            int defence = _runs.Get2(r).Garrison + Rallied(balance);

            if (defence >= balance.SiegeStrength)
            {
                _world.NewEntity().Get<VictoryEvent>().Defence = defence;
            }
            else
            {
                ref var death = ref _world.NewEntity().Get<DeathEvent>();
                death.Cause = DeathCause.Siege;
                death.KillerLordId = -1;
                death.Detail = string.Empty;
            }
        }
    }

    /// <summary>Любовник приходит на более мягких условиях: ему хватает меньшего мнения.</summary>
    private int Rallied(BalanceConfig balance)
    {
        int total = 0;

        foreach (var i in _lords)
        {
            int threshold = _lords.GetEntity(i).Has<LoverFlag>()
                ? balance.LoverComeAtOpinion
                : balance.TroopsComeAtOpinion;

            if (_lords.Get2(i).Value >= threshold) total += _lords.Get3(i).Value;
        }

        return total;
    }
}
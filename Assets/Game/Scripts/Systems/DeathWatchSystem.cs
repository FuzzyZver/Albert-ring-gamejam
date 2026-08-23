using Leopotam.Ecs;

/// <summary>
/// Единственное место, где игрок умирает. Проверяет условия в конце ночи,
/// уже после ночного счёта — поэтому голод и налоги успевают сработать.
///
/// Порядок проверок не случаен: сперва то, что ты сделал сам и видел заранее
/// (бунт, голод, заговор лордов), потом броски. Так смерть почти всегда
/// читается как своя ошибка, а не как невезение.
/// </summary>
public class DeathWatchSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<PhaseEndedEvent> _phaseEnds;
    private EcsFilter<RunFlag, CommonsAttribute, StarvingAttribute>.Exclude<RunOverFlag> _runs;
    private EcsFilter<LordFlag, LordIdAttribute, OpinionAttribute>.Exclude<DeadFlag, LeftCourtFlag> _lords;
    private EcsFilter<PlayerFlag, TraitsAttribute> _players;

    public void Run()
    {
        foreach (var i in _phaseEnds)
        {
            if (_phaseEnds.Get1(i).Phase != DayPhase.Night) continue;
            Check();
        }
    }

    private void Check()
    {
        var balance = GameConfig.BalanceConfig;

        foreach (var r in _runs)
        {
            if (_runs.Get2(r).Opinion <= balance.RiotBelowCommons)
            {
                Kill(DeathCause.Riot, -1, string.Empty);
                return;
            }

            if (_runs.Get3(r).Nights >= balance.FamineNightsToDeath)
            {
                Kill(DeathCause.Famine, -1, string.Empty);
                return;
            }

            if (CountAngry(balance.OverthrowBelowOpinion) >= balance.OverthrowLordsCount)
            {
                Kill(DeathCause.Overthrow, -1, string.Empty);
                return;
            }

            if (TryAssassination(r, balance)) return;
            if (TryOwnTrait(r)) return;
        }
    }

    // ─────────────────────── условия ───────────────────────

    private int CountAngry(int threshold)
    {
        int count = 0;

        foreach (var i in _lords)
            if (_lords.Get3(i).Value <= threshold) count++;

        return count;
    }

    /// <summary>Ножа заслуживают двое: тот, кто уже готовит заговор,
    /// и тот, кого ты довёл настолько, что заговор ему не нужен.</summary>
    private bool TryAssassination(int runIndex, BalanceConfig balance)
    {
        var rng = _runs.GetEntity(runIndex).Get<RngAttribute>().Value;
        if (rng == null) return false;

        foreach (var i in _lords)
        {
            var lord = _lords.GetEntity(i);
            bool plotting = lord.Has<PlottingFlag>();
            bool furious = _lords.Get3(i).Value <= balance.AssassinationBelowOpinion;
            if (!plotting && !furious) continue;

            if (rng.Next(100) >= balance.AssassinationChance) continue;

            Kill(DeathCause.Assassination, _lords.Get2(i).Value, string.Empty);
            return true;
        }

        return false;
    }

    /// <summary>Собственные черты. Если последствие смертельно — эпилог,
    /// если нет — просто последствие, и ночь продолжается.</summary>
    private bool TryOwnTrait(int runIndex)
    {
        var chars = GameConfig.CharactersConfig;
        var rng = _runs.GetEntity(runIndex).Get<RngAttribute>().Value;
        if (rng == null) return false;

        foreach (var p in _players)
        {
            ref var traits = ref _players.Get2(p);

            if (Roll(chars.GetTrait(traits.A), rng)) return true;
            if (Roll(chars.GetTrait(traits.B), rng)) return true;
        }

        return false;
    }

    private bool Roll(TraitDefinition trait, System.Random rng)
    {
        if (trait == null || trait.SelfRisk == ConsequenceId.None) return false;
        if (rng.Next(100) >= trait.SelfRiskChance) return false;

        var consequence = GameConfig.CharactersConfig.GetConsequence(trait.SelfRisk);
        if (consequence == null) return false;

        if (!consequence.IsLethalForPlayer)
        {
            ref var request = ref _world.NewEntity().Get<ConsequenceEvent>();
            request.Id = trait.SelfRisk;
            return false;
        }

        Kill(DeathCause.Accident, -1, consequence.ChronicleLine);
        return true;
    }

    private void Kill(DeathCause cause, int killerLordId, string detail)
    {
        ref var death = ref _world.NewEntity().Get<DeathEvent>();
        death.Cause = cause;
        death.KillerLordId = killerLordId;
        death.Detail = detail;
    }
}
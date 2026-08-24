using Leopotam.Ecs;

/// <summary>
/// Единственное место, где умирает игрок. Смерти делятся на два сорта, и проверяются
/// они по-разному.
///
/// ПОРОГОВЫЕ — бунт и свержение. Это условия на цифры, которые игрок видит на экране,
/// поэтому проверяются каждый кадр: толпа приходит ровно тогда, когда мнение
/// пересекло черту, а не когда ты соберёшься спать.
///
/// НОЧНЫЕ — голод, нож и собственная черта. Это броски, им место раз в ночь.
///
/// Последствия сюда не заглядывают: событие у ворот только двигает мнение,
/// а убивает или нет — решает эта система по своему порогу.
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
        if (!PlayerExists()) return;   // до выбора персонажа умирать некому

        if (CheckThresholds()) return;

        foreach (var i in _phaseEnds)
        {
            if (_phaseEnds.Get1(i).Phase != DayPhase.Night) continue;
            if (CheckNightly()) return;
        }
    }

    // ─────────────────────── пороги ───────────────────────

    private bool CheckThresholds()
    {
        var balance = GameConfig.BalanceConfig;

        foreach (var r in _runs)
        {
            int commons = _runs.Get2(r).Opinion;
            Warn(_runs.GetEntity(r), commons, balance);

            if (commons <= balance.RiotBelowCommons)
            {
                Kill(DeathCause.Riot, -1, string.Empty);
                return true;
            }

            if (CountAngry(balance.OverthrowBelowOpinion) >= balance.OverthrowLordsCount)
            {
                Kill(DeathCause.Overthrow, -1, string.Empty);
                return true;
            }
        }

        return false;
    }

    /// <summary>Один раз предупредить, когда мнение вошло в опасную зону,
    /// и снять предупреждение, когда вышло. Иначе бунт выглядит как гром среди ясного неба.</summary>
    private void Warn(EcsEntity run, int commons, BalanceConfig balance)
    {
        ref var memory = ref run.Get<CommonsMemoryAttribute>();
        bool danger = commons <= balance.RiotBelowCommons + balance.RiotWarningMargin;

        if (danger == memory.Warned) return;
        memory.Warned = danger;

        _world.NewEntity().Get<ChronicleEvent>().Line = danger
            ? "Крестьяне перестали здороваться. На тебя смотрят и считают."
            : "В деревнях снова здороваются. Пока.";
    }

    private int CountAngry(int threshold)
    {
        int count = 0;

        foreach (var i in _lords)
            if (_lords.Get3(i).Value <= threshold) count++;

        return count;
    }

    // ─────────────────────── ночные броски ───────────────────────

    private bool CheckNightly()
    {
        var balance = GameConfig.BalanceConfig;

        foreach (var r in _runs)
        {
            if (_runs.Get3(r).Nights >= balance.FamineNightsToDeath)
            {
                Kill(DeathCause.Famine, -1, string.Empty);
                return true;
            }

            if (TryAssassination(r, balance)) return true;
            if (TryOwnTrait(r)) return true;
        }

        return false;
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
            bool vengeful = lord.Has<VengefulFlag>();

            int chance =
                plotting || furious ? balance.AssassinationChance :
                vengeful ? balance.VengefulAssassinationChance : 0;

            if (chance <= 0 || rng.Next(100) >= chance) continue;

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
            _world.NewEntity().Get<ConsequenceEvent>().Id = trait.SelfRisk;
            return false;
        }

        Kill(DeathCause.Accident, -1, consequence.ChronicleLine);
        return true;
    }

    // ─────────────────────── мелочи ───────────────────────

    private bool PlayerExists()
    {
        foreach (var _ in _players) return true;
        return false;
    }

    private void Kill(DeathCause cause, int killerLordId, string detail)
    {
        ref var death = ref _world.NewEntity().Get<DeathEvent>();
        death.Cause = cause;
        death.KillerLordId = killerLordId;
        death.Detail = detail;
    }
}
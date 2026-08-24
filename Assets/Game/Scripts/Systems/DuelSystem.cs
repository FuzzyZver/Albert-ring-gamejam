using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Поединок. Вызов прилетает днём последствием ChallengeToDuel и висит до вечера,
/// а вечером занимает экран событий: сперва показывается перчатка и твои проценты,
/// и только когда ты нажмёшь — бросок. Пока ждём выбора, фаза заперта PhaseLockFlag,
/// чтобы нельзя было просто пролистнуть вечер.
///
/// Когда дойдём до мини-схваток, менять придётся только Resolve.
/// </summary>
public class DuelSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<ConsequenceEvent> _consequences;
    private EcsFilter<RunFlag, DuelAttribute, EveningAttribute>.Exclude<RunOverFlag> _runs;
    private EcsFilter<LordFlag, LordIdAttribute, PersonAttribute, TraitsAttribute> _lords;
    private EcsFilter<PlayerFlag, TraitsAttribute> _players;

    public void Run()
    {
        foreach (var i in _consequences)
        {
            if (_consequences.Get1(i).Id != ConsequenceId.ChallengeToDuel) continue;
            Challenge(_consequences.Get1(i).Source);
        }

        Announce();

        Resolve();
    }

    // ─────────────────────── вызов ───────────────────────

    private void Challenge(EcsEntity lord)
    {
        if (!lord.IsAlive() || !lord.Has<LordIdAttribute>()) return;

        foreach (var r in _runs)
        {
            ref var duel = ref _runs.Get2(r);
            if (duel.LordId >= 0) continue;   // одна перчатка за раз

            duel.LordId = lord.Get<LordIdAttribute>().Value;
            Chronicle($"{lord.Get<PersonAttribute>().GivenName} бросил перчатку. Вечером во дворе.");
        }
    }

    // ─────────────────────── вечер ───────────────────────

    /// <summary>Очередь вечера дошла до поединка — наполняем текст.
    /// Замком фазы заведует EveningSystem, здесь его не трогаем.</summary>
    private void Announce()
    {
        var balance = GameConfig.BalanceConfig;
        var chars = GameConfig.CharactersConfig;

        foreach (var r in _runs)
        {
            ref var duel = ref _runs.Get2(r);
            ref var evening = ref _runs.Get3(r);

            if (evening.Kind != EveningKind.Duel || !evening.Waiting) continue;
            if (!string.IsNullOrEmpty(evening.Body)) continue;   // уже наполнено

            if (duel.LordId < 0 || !TryFindLord(duel.LordId, out int index)
                || _lords.GetEntity(index).Has<DeadFlag>())
            {
                duel.LordId = -1;          // противник не дожил до вечера
                evening.Waiting = false;
                continue;
            }

            duel.Chance = Mathf.Clamp(
                balance.DuelWinChanceBase + PlayerSkill(chars) - LordSkill(chars, index),
                balance.DuelChanceMin, balance.DuelChanceMax);

            evening.Title = "Поединок";
            evening.Body = GameConfig.EventsConfig.DuelChallengeText
                .Replace("{lord}", _lords.Get3(index).GivenName)
                .Replace("{chance}", duel.Chance.ToString());
        }
    }

    private void Resolve()
    {
        var balance = GameConfig.BalanceConfig;

        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var duel = ref _runs.Get2(r);
            ref var evening = ref _runs.Get3(r);

            if (!entity.Has<DuelAcceptedFlag>()) continue;
            entity.Del<DuelAcceptedFlag>();

            if (duel.LordId < 0 || evening.Kind != EveningKind.Duel) continue;
            if (!TryFindLord(duel.LordId, out int index)) { Release(entity, ref duel, ref evening); continue; }

            var lord = _lords.GetEntity(index);
            string name = _lords.Get3(index).GivenName;

            var rng = entity.Get<RngAttribute>().Value;
            bool won = rng == null || rng.Next(100) < duel.Chance;

            if (won)
            {
                lord.Get<DeadFlag>();
                lord.Get<LeftCourtFlag>();

                evening.Result = GameConfig.EventsConfig.DuelWinText.Replace("{lord}", name);
                Chronicle($"Поединок с {name} — ты выстоял. Копий его больше нет.");
            }
            else
            {
                ref var death = ref _world.NewEntity().Get<DeathEvent>();
                death.Cause = DeathCause.Duel;
                death.KillerLordId = duel.LordId;
                death.Detail = string.Empty;
            }

            Release(entity, ref duel, ref evening);
        }
    }

    private static void Release(EcsEntity run, ref DuelAttribute duel, ref EveningAttribute evening)
    {
        duel.LordId = -1;
        evening.Waiting = false;
        evening.ShowingResult = !string.IsNullOrEmpty(evening.Result);
    }

    // ─────────────────────── мастерство ───────────────────────

    private int PlayerSkill(CharactersConfig chars)
    {
        foreach (var p in _players)
        {
            ref var traits = ref _players.Get2(p);
            return Skill(chars, traits.A) + Skill(chars, traits.B);
        }

        return 0;
    }

    private int LordSkill(CharactersConfig chars, int lordIndex)
    {
        ref var traits = ref _lords.Get4(lordIndex);
        return Skill(chars, traits.A) + Skill(chars, traits.B);
    }

    private static int Skill(CharactersConfig chars, TraitId id)
    {
        var trait = chars.GetTrait(id);
        return trait != null ? trait.DuelChance : 0;
    }

    private bool TryFindLord(int lordId, out int index)
    {
        foreach (var i in _lords)
        {
            if (_lords.Get2(i).Value != lordId) continue;
            index = i;
            return true;
        }

        index = -1;
        return false;
    }

    private void Chronicle(string line) => _world.NewEntity().Get<ChronicleEvent>().Line = line;
}
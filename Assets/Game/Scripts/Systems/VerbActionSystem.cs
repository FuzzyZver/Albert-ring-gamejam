using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Применение глагола. Формулы здесь нет вообще: берётся та самая строка,
/// которую игрок видел в карточке. Если строки нет или она была недоступна —
/// ничего не происходит. Сделать то, чего тебе не показали, нельзя.
/// </summary>
public class VerbActionSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<VerbEvent> _requests;
    private EcsFilter<RunFlag, SelectionAttribute, VerbOffersAttribute, CalendarAttribute> _runs;
    private EcsFilter<LordFlag, LordIdAttribute, PersonAttribute> _lords;

    public void Run()
    {
        foreach (var i in _requests)
        {
            var request = _requests.Get1(i);

            foreach (var r in _runs)
            {
                ref var selection = ref _runs.Get2(r);
                if (selection.LordId != request.TargetLordId) continue;   // строки считались не для него

                if (!TryTakeOffer(r, request.Verb, out var outcome)) continue;
                if (!outcome.Available) continue;
                if (!TryFindLord(request.TargetLordId, out var target)) continue;

                Perform(r, outcome, target);
            }
        }
    }

    private void Perform(int runIndex, VerbOutcome outcome, EcsEntity target)
    {
        Pay(runIndex, outcome, target);

        bool success = !outcome.IsChanceBased || Roll(runIndex) < outcome.Chance;
        if (success) Succeed(runIndex, outcome, target);
        else Fail(outcome, target);

        FireConsequences(runIndex, outcome, target);

        _runs.GetEntity(runIndex).Get<SelectionChangedFlag>();   // цифры изменились, пересчитать
    }

    // ─────────────────────── цена ───────────────────────

    private void Pay(int runIndex, VerbOutcome outcome, EcsEntity target)
    {
        var entity = _runs.GetEntity(runIndex);
        ref var calendar = ref _runs.Get4(runIndex);
        ref var treasury = ref entity.Get<TreasuryAttribute>();

        treasury.Gold -= outcome.GoldCost;
        treasury.Food -= outcome.FoodCost;
        calendar.ActionsLeft--;

        // Пишем каждое применение: по этой истории считается и «один раз за забег»,
        // и кулдаун, и то, насколько лесть уже приелась.
        var history = target.Get<VerbHistoryAttribute>().Value;
        if (history != null) history.Add(new VerbUse { Verb = outcome.Verb, Day = calendar.Day });
    }

    // ─────────────────────── успех ───────────────────────

    private void Succeed(int runIndex, VerbOutcome outcome, EcsEntity target)
    {
        Opinion(target, outcome.Opinion, outcome.Title);

        int rivalId = target.Get<RivalAttribute>().LordId;
        if (outcome.RivalOpinion != 0 && rivalId >= 0 && TryFindLord(rivalId, out var rival))
            Opinion(rival, outcome.RivalOpinion, "из-за соседа");

        if (outcome.CommonsOpinion != 0)
        {
            ref var commons = ref _world.NewEntity().Get<CommonsOpinionChangeEvent>();
            commons.Delta = outcome.CommonsOpinion;
            commons.Reason = outcome.Title;
        }

        if (outcome.CourtOpinion != 0)
        {
            ref var court = ref _world.NewEntity().Get<CourtOpinionChangeEvent>();
            court.Delta = outcome.CourtOpinion;
            court.ExceptLordId = target.Get<LordIdAttribute>().Value;
            court.Reason = outcome.Title;
        }

        ApplySpecials(runIndex, outcome.Verb, target, outcome.TroopsGained);
        Chronicle($"{Name(target)}: {outcome.Title} — {Signed(outcome.Opinion)}");
    }

    /// <summary>Глаголы, которые меняют не только цифры, но и состояние.</summary>
    private void ApplySpecials(int runIndex, VerbId verb, EcsEntity target, int troops)
    {
        switch (verb)
        {
            case VerbId.Seduce:
                target.Get<LoverFlag>();
                Chronicle($"{Name(target)} теперь твой любовник. Это надолго.");
                break;

            case VerbId.FulfillAmbition:
                target.Get<AmbitionFulfilledFlag>();
                break;

            case VerbId.InviteToCastle:
                if (target.Has<LeftCourtFlag>())
                {
                    target.Del<LeftCourtFlag>();   // уехавшего можно вернуть — за стол и за деньги
                    Chronicle($"{Name(target)} вернулся ко двору. Копья вернулись с ним.");
                }
                break;

            case VerbId.AskForTroops:
                {
                    if (troops <= 0) break;

                    ref var lordTroops = ref target.Get<TroopsAttribute>();
                    ref var treasury = ref _runs.GetEntity(runIndex).Get<TreasuryAttribute>();

                    int sent = Mathf.Min(troops, lordTroops.Value);
                    lordTroops.Value -= sent;
                    treasury.Garrison += sent;

                    Chronicle($"{Name(target)} прислал {sent} копий. Теперь их надо кормить.");
                    break;
                }

            case VerbId.HuntTogether:
                {
                    ref var calendar = ref _runs.Get4(runIndex);
                    ref var plan = ref _runs.GetEntity(runIndex).Get<PlanAttribute>();
                    plan.HasPlan = true;
                    plan.PlannedOnDay = calendar.Day;
                    Chronicle("Охота назначена на завтрашний вечер.");
                    break;
                }
        }
    }

    // ─────────────────────── провал ───────────────────────

    private void Fail(VerbOutcome outcome, EcsEntity target)
    {
        Chronicle($"{Name(target)}: {outcome.Title} — отказ.");
        Opinion(target, outcome.OpinionOnFail, "отказ");

        if (outcome.OnFail == ConsequenceId.None) return;

        ref var consequence = ref _world.NewEntity().Get<ConsequenceEvent>();
        consequence.Source = target;
        consequence.Id = outcome.OnFail;
    }

    /// <summary>Реакции черт. Летят независимо от того, удался глагол:
    /// Гордый вызывает на поединок именно потому, что ты успешно ему пригрозил.</summary>
    private void FireConsequences(int runIndex, VerbOutcome outcome, EcsEntity target)
    {
        if (outcome.Consequences == null) return;

        for (int i = 0; i < outcome.Consequences.Count; i++)
        {
            var pending = outcome.Consequences[i];
            if (pending.Chance < 100 && Roll(runIndex) >= pending.Chance) continue;

            ref var request = ref _world.NewEntity().Get<ConsequenceEvent>();
            request.Source = target;
            request.Id = pending.Id;
        }
    }

    // ─────────────────────── мелочи ───────────────────────

    private bool TryTakeOffer(int runIndex, VerbId verb, out VerbOutcome outcome)
    {
        var offers = _runs.Get3(runIndex).Value;

        if (offers != null)
        {
            for (int i = 0; i < offers.Count; i++)
            {
                if (offers[i].Verb != verb) continue;
                outcome = offers[i];
                return true;
            }
        }

        outcome = default;
        return false;
    }

    private void Opinion(EcsEntity target, int delta, string reason)
    {
        if (delta == 0) return;

        ref var change = ref _world.NewEntity().Get<OpinionChangeEvent>();
        change.Target = target;
        change.Delta = delta;
        change.Reason = reason;
    }

    private int Roll(int runIndex)
    {
        var rng = _runs.GetEntity(runIndex).Get<RngAttribute>().Value;
        return rng != null ? rng.Next(100) : 0;
    }

    private bool TryFindLord(int lordId, out EcsEntity found)
    {
        foreach (var i in _lords)
        {
            if (_lords.Get2(i).Value != lordId) continue;
            found = _lords.GetEntity(i);
            return true;
        }

        found = default;
        return false;
    }

    private static string Name(EcsEntity entity) => entity.Get<PersonAttribute>().GivenName;

    private static string Signed(int value) => value > 0 ? "+" + value : value.ToString();

    private void Chronicle(string line) => _world.NewEntity().Get<ChronicleEvent>().Line = line;
}
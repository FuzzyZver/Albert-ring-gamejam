using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Формула глагола. Единственное место в проекте, где она живёт.
/// Считает разом все строки для выделенного лорда и кладёт их в VerbOffersAttribute:
/// панель их рисует, VerbActionSystem их применяет. Никто ничего не пересчитывает,
/// поэтому карточка физически не может пообещать одно, а сделать другое.
///
/// Флаг SelectionChangedFlag система НЕ снимает — его снимает OneFrame в конце кадра,
/// когда вьюхи уже успели перерисоваться.
/// </summary>
public class VerbResolveSystem : Injects, IEcsRunSystem
{
    private EcsFilter<RunFlag, SelectionAttribute, VerbOffersAttribute, CalendarAttribute> _runs;
    private EcsFilter<PlayerFlag, TraitsAttribute> _players;
    private EcsFilter<LordFlag, LordIdAttribute, PersonAttribute> _lords;

    private readonly StringBuilder _text = new StringBuilder();

    public void Run()
    {
        foreach (var r in _runs)
        {
            if (!_runs.GetEntity(r).Has<SelectionChangedFlag>()) continue;

            var offers = _runs.Get3(r).Value;
            if (offers == null) continue;

            offers.Clear();

            ref var selection = ref _runs.Get2(r);
            if (!selection.HasTarget || selection.IsPlayer) continue;
            if (!TryFindLord(selection.LordId, out var target)) continue;

            var context = BuildContext(r, target);
            var verbs = GameConfig.CharactersConfig.Verbs;

            for (int v = 0; v < verbs.Length; v++)
                offers.Add(Resolve(verbs[v].Id, context));
        }
    }

    // ─────────────────────── контекст ───────────────────────

    private struct Context
    {
        public TraitId PlayerA, PlayerB;
        public TraitId TargetA, TargetB;
        public Gender TargetGender;
        public AmbitionId Ambition;
        public bool AmbitionDone;
        public bool HasRival;
        public string RivalName;
        public int TargetTroops;
        public int TargetOpinion;
        public int Gold, Food, Actions, Day;
        public bool IsDay;
        public bool RomanceClosed;
        public List<VerbUse> History;

        public bool PlayerHas(TraitId id) => PlayerA == id || PlayerB == id;

        public int Uses(VerbId verb)
        {
            if (History == null) return 0;

            int count = 0;
            for (int i = 0; i < History.Count; i++)
                if (History[i].Verb == verb) count++;

            return count;
        }

        /// <summary>int.MinValue, если ещё ни разу.</summary>
        public int LastDay(VerbId verb)
        {
            int last = int.MinValue;
            if (History == null) return last;

            for (int i = 0; i < History.Count; i++)
                if (History[i].Verb == verb && History[i].Day > last) last = History[i].Day;

            return last;
        }
    }

    private Context BuildContext(int runIndex, EcsEntity target)
    {
        var context = new Context();

        ref var traits = ref target.Get<TraitsAttribute>();
        context.TargetA = traits.A;
        context.TargetB = traits.B;
        context.TargetGender = target.Get<PersonAttribute>().Gender;
        context.Ambition = target.Get<AmbitionAttribute>().Id;
        context.AmbitionDone = target.Has<AmbitionFulfilledFlag>();

        var ambition = GameConfig.CharactersConfig.GetAmbition(context.Ambition);
        context.RomanceClosed = context.AmbitionDone && ambition != null && ambition.ClosesRomance;
        context.History = target.Get<VerbHistoryAttribute>().Value;
        context.TargetTroops = target.Get<TroopsAttribute>().Value;
        context.TargetOpinion = target.Get<OpinionAttribute>().Value;

        int rivalId = target.Get<RivalAttribute>().LordId;
        context.HasRival = rivalId >= 0;
        context.RivalName = context.HasRival ? ShortName(rivalId) : string.Empty;

        foreach (var p in _players)
        {
            ref var playerTraits = ref _players.Get2(p);
            context.PlayerA = playerTraits.A;
            context.PlayerB = playerTraits.B;
            break;
        }

        var entity = _runs.GetEntity(runIndex);
        ref var calendar = ref _runs.Get4(runIndex);
        ref var treasury = ref entity.Get<TreasuryAttribute>();

        context.Gold = treasury.Gold;
        context.Food = treasury.Food;
        context.Actions = calendar.ActionsLeft;
        context.Day = calendar.Day;
        context.IsDay = calendar.Phase == DayPhase.Day;

        return context;
    }

    // ─────────────────────── формула ───────────────────────

    private VerbOutcome Resolve(VerbId verb, Context context)
    {
        var chars = GameConfig.CharactersConfig;
        var definition = chars.GetVerb(verb);

        var outcome = new VerbOutcome { Verb = verb, Available = true };
        if (definition == null)
        {
            outcome.Title = verb.ToString();
            outcome.Available = false;
            outcome.Blocked = "нет описания в конфиге";
            return outcome;
        }

        var ambition = verb == VerbId.FulfillAmbition
            ? chars.GetAmbition(context.Ambition)
            : null;

        outcome.Title = ambition != null && !string.IsNullOrEmpty(ambition.PlayerAction)
            ? ambition.PlayerAction
            : definition.Title;

        outcome.Opinion = definition.BaseOpinion;
        outcome.Chance = definition.BaseChance;
        outcome.GoldCost = definition.GoldCost;
        outcome.FoodCost = definition.FoodCost;
        outcome.RivalOpinion = definition.RivalOpinion;
        outcome.CommonsOpinion = definition.CommonsOpinion;
        outcome.CourtOpinion = definition.CourtOpinion;
        outcome.OnFail = definition.OnFail;
        outcome.OpinionOnFail = definition.OpinionOnFail;

        // Просьба о войске — единственный глагол, который двигает копья.
        // Лорд отдаёт часть сразу: они защищают стены, но и едят каждую ночь.
        // И он вправе отказать: шанс растёт от мнения, при +30 уже прилично.
        if (verb == VerbId.AskForTroops)
        {
            var balance = GameConfig.BalanceConfig;
            outcome.Chance = balance.TroopsChance(context.TargetOpinion);

            if (context.TargetTroops > 0)
                outcome.TroopsGained = Mathf.Max(1, context.TargetTroops * balance.TroopsPercentOnRequest / 100);
        }

        if (ambition != null)
        {
            outcome.Opinion += ambition.OpinionOnFulfill;
            outcome.GoldCost += ambition.GoldCost;
            outcome.CommonsOpinion += ambition.CommonsOpinion;
            outcome.CourtOpinion += ambition.CourtOpinion;
        }

        _text.Length = 0;

        ApplyReaction(chars.GetTrait(context.TargetA), verb, context.TargetGender, false, ref outcome);
        ApplyReaction(chars.GetTrait(context.TargetB), verb, context.TargetGender, false, ref outcome);
        ApplyReaction(chars.GetTrait(context.PlayerA), verb, Gender.Male, true, ref outcome);
        ApplyReaction(chars.GetTrait(context.PlayerB), verb, Gender.Male, true, ref outcome);

        int repeats = context.Uses(verb);
        if (repeats > 0 && definition.RepeatPenalty > 0)
        {
            int penalty = definition.RepeatPenalty * repeats;
            outcome.Opinion -= penalty;
            Separate();
            _text.Append("приедается −").Append(penalty);
        }

        outcome.IsChanceBased = outcome.Chance < 100;
        outcome.Chance = outcome.IsChanceBased ? Mathf.Clamp(outcome.Chance, 5, 95) : 100;

        AppendSideEffects(context, ref outcome);
        AppendFailure(chars, ref outcome);

        outcome.Breakdown = _text.ToString();
        outcome.CostLine = BuildCostLine(definition, ambition, chars);

        Block(context, definition, ambition, ref outcome);
        return outcome;
    }

    private void ApplyReaction(TraitDefinition trait, VerbId verb, Gender gender, bool self, ref VerbOutcome outcome)
    {
        if (trait == null) return;

        var reaction = self ? trait.GetSelfReaction(verb) : trait.GetReaction(verb);
        if (reaction.IsEmpty && string.IsNullOrEmpty(reaction.Note)) return;

        outcome.Opinion += reaction.Opinion;
        outcome.Chance += reaction.Chance;

        if (reaction.Consequence != ConsequenceId.None)
        {
            if (outcome.Consequences == null) outcome.Consequences = new List<VerbConsequence>();
            outcome.Consequences.Add(new VerbConsequence
            {
                Id = reaction.Consequence,
                Chance = reaction.RealChance,
            });
        }

        Separate();
        _text.Append(trait.GetTitle(gender));

        if (reaction.Opinion != 0) _text.Append(' ').Append(Signed(reaction.Opinion));
        if (reaction.Chance != 0) _text.Append(' ').Append(Signed(reaction.Chance)).Append(" к шансу");
        if (!string.IsNullOrEmpty(reaction.Note)) _text.Append(" — ").Append(reaction.Note);
    }

    private void AppendSideEffects(Context context, ref VerbOutcome outcome)
    {
        if (outcome.TroopsGained > 0)
        {
            Separate();
            _text.Append('+').Append(outcome.TroopsGained).Append(" копий — их надо кормить");
        }

        if (outcome.RivalOpinion != 0 && context.HasRival)
        {
            Separate();
            _text.Append(context.RivalName).Append(' ').Append(Signed(outcome.RivalOpinion));
        }

        if (outcome.CommonsOpinion != 0)
        {
            Separate();
            _text.Append("крестьяне ").Append(Signed(outcome.CommonsOpinion));
        }

        if (outcome.CourtOpinion != 0)
        {
            Separate();
            _text.Append("двор ").Append(Signed(outcome.CourtOpinion));
        }
    }

    private void AppendFailure(CharactersConfig chars, ref VerbOutcome outcome)
    {
        if (outcome.IsChanceBased && outcome.OpinionOnFail != 0)
        {
            Separate();
            _text.Append("отказ ").Append(Signed(outcome.OpinionOnFail));
        }

        if (outcome.OnFail == ConsequenceId.None) return;

        var consequence = chars.GetConsequence(outcome.OnFail);
        if (consequence == null || string.IsNullOrEmpty(consequence.Title)) return;

        Separate();
        _text.Append("провал = ").Append(consequence.Title);
    }

    // ─────────────────────── доступность ───────────────────────

    private static void Block(Context context, VerbDefinition definition,
        AmbitionDefinition ambition, ref VerbOutcome outcome)
    {
        if (!context.IsDay) Deny(ref outcome, "не время для разговоров");
        else if (context.Actions <= 0) Deny(ref outcome, "действий не осталось");
        else if (definition.OncePerLord && context.Uses(definition.Id) > 0) Deny(ref outcome, "уже было");
        else if (definition.CooldownDays > 0
                 && context.Day - context.LastDay(definition.Id) < definition.CooldownDays)
            Deny(ref outcome, "просил сегодня");
        else if (definition.Id == VerbId.FulfillAmbition && context.AmbitionDone) Deny(ref outcome, "уже исполнено");
        else if (definition.Id == VerbId.FulfillAmbition && ambition == null) Deny(ref outcome, "желания нет");
        else if (definition.Id == VerbId.AskForTroops && context.TargetTroops <= 0) Deny(ref outcome, "копий у него нет");
        else if (definition.Id == VerbId.Seduce && context.RomanceClosed)
            Deny(ref outcome, "ты женат на его дочери");
        else if (definition.RequiresTrait && !context.PlayerHas(definition.RequiredTrait))
            Deny(ref outcome, "нужна черта");
        else if (outcome.GoldCost > context.Gold) Deny(ref outcome, "не хватает золота");
        else if (outcome.FoodCost > context.Food) Deny(ref outcome, "не хватает пищи");
    }

    private static void Deny(ref VerbOutcome outcome, string reason)
    {
        outcome.Available = false;
        outcome.Blocked = reason;
    }

    private string BuildCostLine(VerbDefinition definition, AmbitionDefinition ambition, CharactersConfig chars)
    {
        _text.Length = 0;

        if (ambition != null) Separate("его желание · один раз");

        int gold = definition.GoldCost + (ambition != null ? ambition.GoldCost : 0);
        if (gold > 0) Separate(gold + " золота");
        if (definition.FoodCost > 0) Separate(definition.FoodCost + " пищи");

        if (definition.RequiresTrait)
        {
            var trait = chars.GetTrait(definition.RequiredTrait);
            Separate("черта: " + (trait != null ? trait.Title : definition.RequiredTrait.ToString()));
        }

        if (definition.OncePerLord && ambition == null) Separate("один раз");
        if (definition.CooldownDays > 0) Separate("раз в " + definition.CooldownDays + " дн.");

        return _text.Length > 0 ? _text.ToString() : "бесплатно";
    }

    // ─────────────────────── мелочи ───────────────────────

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

    private string ShortName(int lordId)
    {
        foreach (var i in _lords)
            if (_lords.Get2(i).Value == lordId)
                return _lords.Get3(i).GivenName;

        return string.Empty;
    }

    private void Separate(string part = null)
    {
        if (_text.Length > 0) _text.Append(" · ");
        if (part != null) _text.Append(part);
    }

    private static string Signed(int value) => value > 0 ? "+" + value : value.ToString();
}
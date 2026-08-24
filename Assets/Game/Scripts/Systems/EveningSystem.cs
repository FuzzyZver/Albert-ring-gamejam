using System.Collections.Generic;
using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Вечер. Собирает очередь и проигрывает её по одному событию: сперва то, что
/// заготовили за день (пир), потом одно событие по триггерам, последним — поединок.
/// Сама событий не выдумывает сверх этого: любая система может доложить в очередь
/// через QueueEveningEvent, и оно сыграет своим чередом.
///
/// Рисует не она, а EveningViewSystem — потому что тело поединка дописывает
/// DuelSystem, которая стоит в конвейере позже.
/// </summary>
public class EveningSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<PhaseChangedEvent> _phaseStarts;
    private EcsFilter<PhaseEndedEvent> _phaseEnds;
    private EcsFilter<QueueEveningEvent> _queued;
    private EcsFilter<EveningChoiceEvent> _choices;

    private EcsFilter<RunFlag, EveningQueueAttribute, EveningAttribute, CalendarAttribute> _runs;
    private EcsFilter<PlayerFlag, TraitsAttribute> _players;
    private EcsFilter<LordFlag, LordIdAttribute, PersonAttribute, TraitsAttribute>.Exclude<DeadFlag, LeftCourtFlag> _lords;

    private readonly List<EveningEventId> _eligible = new List<EveningEventId>();
    private readonly List<int> _weights = new List<int>();

    public void Run()
    {
        foreach (var i in _queued) Push(_queued.Get1(i));

        foreach (var i in _phaseStarts)
            if (_phaseStarts.Get1(i).Phase == DayPhase.Evening) Begin();

        foreach (var i in _phaseEnds)
            if (_phaseEnds.Get1(i).Phase == DayPhase.Evening) Finish();

        foreach (var i in _choices) Choose(_choices.Get1(i).Index);

        Advance();
    }

    // ─────────────────────── сбор очереди ───────────────────────

    private void Push(QueueEveningEvent request)
    {
        foreach (var r in _runs)
            _runs.Get2(r).Value.Add(new EveningEntry
            {
                Kind = request.Kind,
                Id = request.Id,
                LordId = request.LordId,
            });
    }

    private void Begin()
    {
        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var queue = ref _runs.Get2(r);
            ref var calendar = ref _runs.Get4(r);

            AddPlannedFeast(entity, ref queue, calendar.Day);
            AddTriggered(entity, ref queue, calendar.Day);
            AddDuel(entity, ref queue);

            queue.Index = -1;
            queue.Started = true;
            entity.Get<PhaseLockFlag>();
        }
    }

    /// <summary>Сборы, назначенные вчера в замке.</summary>
    private void AddPlannedFeast(EcsEntity run, ref EveningQueueAttribute queue, int day)
    {
        ref var plan = ref run.Get<PlanAttribute>();
        if (!plan.HasPlan || plan.PlannedOnDay >= day) return;

        plan.HasPlan = false;
        queue.Value.Add(new EveningEntry
        {
            Kind = EveningKind.Story,
            Id = EveningEventId.Feast,
            LordId = RandomLord(run),
        });
    }

    /// <summary>Одно событие по триггерам. Вес 0 в конфиге значит «только по прямому вызову».</summary>
    private void AddTriggered(EcsEntity run, ref EveningQueueAttribute queue, int day)
    {
        var events = GameConfig.EventsConfig;
        if (events == null) return;

        var rng = run.Get<RngAttribute>().Value;
        int commons = run.Get<CommonsAttribute>().Opinion;

        _eligible.Clear();
        _weights.Clear();
        int total = 0;

        for (int i = 0; i < events.EveningEvents.Length; i++)
        {
            var definition = events.EveningEvents[i];
            if (definition.Weight <= 0 || day < definition.MinDay) continue;
            if (commons > definition.MaxCommons) continue;
            if (definition.NeedsPlayerTrait && !PlayerHas(definition.PlayerTrait)) continue;
            if (definition.NeedsCourtTrait && FindLordWith(definition.CourtTrait) < 0) continue;
            if (definition.NeedsLover && FindLover() < 0) continue;

            _eligible.Add(definition.Id);
            _weights.Add(definition.Weight);
            total += definition.Weight;
        }

        if (_eligible.Count == 0 || total <= 0 || rng == null) return;

        int roll = rng.Next(total);
        for (int i = 0; i < _eligible.Count; i++)
        {
            roll -= _weights[i];
            if (roll >= 0) continue;

            var definition = events.GetEvening(_eligible[i]);
            queue.Value.Add(new EveningEntry
            {
                Kind = EveningKind.Story,
                Id = _eligible[i],
                LordId = BindLord(run, definition),
            });
            return;
        }
    }

    private void AddDuel(EcsEntity run, ref EveningQueueAttribute queue)
    {
        int lordId = run.Get<DuelAttribute>().LordId;
        if (lordId < 0) return;

        queue.Value.Add(new EveningEntry { Kind = EveningKind.Duel, LordId = lordId });
    }

    // ─────────────────────── проигрывание ───────────────────────

    private void Advance()
    {
        var events = GameConfig.EventsConfig;

        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var queue = ref _runs.Get2(r);
            ref var evening = ref _runs.Get3(r);
            ref var calendar = ref _runs.Get4(r);

            if (calendar.Phase != DayPhase.Evening || !queue.Started) continue;

            // Пока игрок не прочитал итог прошлого события, очередь стоит.
            // Иначе выбор смахивался бы в том же кадре и выглядел как «ничего не произошло».
            if (evening.Waiting || evening.ShowingResult) continue;

            if (queue.Index + 1 >= queue.Value.Count)
            {
                if (entity.Has<PhaseLockFlag>()) entity.Del<PhaseLockFlag>();
                continue;
            }

            queue.Index++;
            var entry = queue.Value[queue.Index];

            evening.Kind = entry.Kind;
            evening.Id = entry.Id;
            evening.LordId = entry.LordId;
            evening.Result = string.Empty;
            evening.Waiting = true;
            evening.ShowingResult = false;

            if (entry.Kind != EveningKind.Story)
            {
                evening.Title = string.Empty;   // тело допишет DuelSystem
                evening.Body = string.Empty;
                continue;
            }

            var definition = events.GetEvening(entry.Id);
            if (definition == null) { evening.Waiting = false; continue; }

            evening.Title = definition.Title;
            evening.Body = events.Fill(definition.Text, PlayerName(), LordName(entry.LordId), string.Empty);
        }
    }

    private void Choose(int index)
    {
        var events = GameConfig.EventsConfig;

        foreach (var r in _runs)
        {
            ref var evening = ref _runs.Get3(r);

            if (evening.ShowingResult)
            {
                evening.ShowingResult = false;   // «Дальше» — пускаем очередь
                continue;
            }

            if (!evening.Waiting) continue;

            if (evening.Kind == EveningKind.Duel)
            {
                if (index == 0) _runs.GetEntity(r).Get<DuelAcceptedFlag>();
                continue;
            }

            if (evening.Kind != EveningKind.Story) continue;

            var definition = events.GetEvening(evening.Id);
            if (definition == null) { evening.Waiting = false; continue; }

            if (definition.Choices.Length == 0)
            {
                evening.Waiting = false;   // просто «дальше»
                continue;
            }

            if (index < 0 || index >= definition.Choices.Length) continue;

            var choice = definition.Choices[index];
            ref var treasury = ref _runs.GetEntity(r).Get<TreasuryAttribute>();
            if (!choice.CanAfford(treasury.Gold, treasury.Food, treasury.Garrison)) continue;

            string result = events.Fill(choice.Result, PlayerName(), LordName(evening.LordId), string.Empty);

            ref var request = ref _world.NewEntity().Get<ApplyChoiceEvent>();
            request.Choice = choice;
            request.LordId = evening.LordId;
            request.Result = result;

            evening.Result = result;
            evening.Waiting = false;
            evening.ShowingResult = !string.IsNullOrEmpty(result);
        }
    }

    private void Finish()
    {
        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var queue = ref _runs.Get2(r);
            ref var evening = ref _runs.Get3(r);

            queue.Value.Clear();
            queue.Index = -1;
            queue.Started = false;

            evening.Kind = EveningKind.None;
            evening.Waiting = false;
            evening.ShowingResult = false;
            evening.LordId = -1;

            if (entity.Has<PhaseLockFlag>()) entity.Del<PhaseLockFlag>();
        }
    }

    // ─────────────────────── поиск ───────────────────────

    private int BindLord(EcsEntity run, EveningEventDefinition definition)
    {
        if (definition == null || !definition.NeedsLord) return -1;
        if (definition.NeedsCourtTrait) return FindLordWith(definition.CourtTrait);
        if (definition.NeedsLover) return FindLover();
        return RandomLord(run);
    }

    private int FindLordWith(TraitId trait)
    {
        foreach (var i in _lords)
        {
            ref var traits = ref _lords.Get4(i);
            if (traits.Has(trait)) return _lords.Get2(i).Value;
        }

        return -1;
    }

    private int FindLover()
    {
        foreach (var i in _lords)
            if (_lords.GetEntity(i).Has<LoverFlag>()) return _lords.Get2(i).Value;

        return -1;
    }

    private int RandomLord(EcsEntity run)
    {
        var rng = run.Get<RngAttribute>().Value;

        int count = 0;
        foreach (var _ in _lords) count++;
        if (count == 0 || rng == null) return -1;

        int pick = rng.Next(count);
        int index = 0;

        foreach (var i in _lords)
        {
            if (index++ != pick) continue;
            return _lords.Get2(i).Value;
        }

        return -1;
    }

    private bool PlayerHas(TraitId trait)
    {
        foreach (var p in _players) return _players.Get2(p).Has(trait);
        return false;
    }

    private string PlayerName()
    {
        foreach (var p in _players) return _players.GetEntity(p).Get<PersonAttribute>().FullName;
        return "государь";
    }

    private string LordName(int lordId)
    {
        if (lordId < 0) return string.Empty;

        foreach (var i in _lords)
            if (_lords.Get2(i).Value == lordId)
                return _lords.Get3(i).FullName;

        return string.Empty;
    }
}
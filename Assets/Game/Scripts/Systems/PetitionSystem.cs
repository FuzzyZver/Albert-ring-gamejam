using System.Collections.Generic;
using Leopotam.Ecs;

/// <summary>
/// Утро у трона. Трое просителей подходят по одному, каждый требует решения.
/// Пока очередь не кончилась, на забеге висит PhaseLockFlag — утро не отпускает.
///
/// Владеет замком фазы только в утренней фазе; вечером им заведует EveningSystem.
/// Два владельца одного флага — верный способ получить намертво запертый день.
/// </summary>
public class PetitionSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<PhaseChangedEvent> _phaseStarts;
    private EcsFilter<RunReadyEvent> _runReady;
    private EcsFilter<CallNextPetitionerEvent> _calls;
    private EcsFilter<PetitionChoiceEvent> _choices;

    private EcsFilter<RunFlag, PetitionQueueAttribute, CalendarAttribute, TreasuryAttribute> _runs;
    private EcsFilter<PlayerFlag, PersonAttribute> _players;
    private EcsFilter<LordFlag, LordIdAttribute, PersonAttribute>.Exclude<DeadFlag, LeftCourtFlag> _lords;

    private readonly List<int> _pool = new List<int>();

    public void Init() => Subscribe(true);
    public void Destroy() => Subscribe(false);

    public void Run()
    {
        foreach (var i in _phaseStarts)
            if (_phaseStarts.Get1(i).Phase == DayPhase.Morning) Fill();

        foreach (var _ in _runReady) Fill();
        foreach (var _ in _calls) CallNext();
        foreach (var i in _choices) Choose(_choices.Get1(i).Index);

        Repaint();
    }

    // ─────────────────────── набор очереди ───────────────────────

    private void Fill()
    {
        var events = GameConfig.EventsConfig;
        var balance = GameConfig.BalanceConfig;
        if (events == null || events.Petitions.Length == 0) return;

        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var queue = ref _runs.Get2(r);

            queue.Value.Clear();
            queue.Index = -1;
            queue.Waiting = false;
            queue.Result = string.Empty;

            var rng = entity.Get<RngAttribute>().Value;
            if (rng == null) continue;

            _pool.Clear();
            for (int i = 0; i < events.Petitions.Length; i++) _pool.Add(i);

            int count = UnityEngine.Mathf.Min(balance.PetitionersPerMorning, _pool.Count);
            for (int taken = 0; taken < count; taken++)
            {
                int pick = rng.Next(_pool.Count);
                var definition = events.Petitions[_pool[pick]];
                _pool.RemoveAt(pick);

                queue.Value.Add(new PetitionEntry
                {
                    Id = definition.Id,
                    LordId = definition.NeedsLord ? RandomLord(rng) : -1,
                });
            }

            entity.Get<PhaseLockFlag>();
        }
    }

    private int RandomLord(System.Random rng)
    {
        int count = 0;
        foreach (var _ in _lords) count++;
        if (count == 0) return -1;

        int pick = rng.Next(count);
        int index = 0;

        foreach (var i in _lords)
        {
            if (index++ != pick) continue;
            return _lords.Get2(i).Value;
        }

        return -1;
    }

    // ─────────────────────── ход очереди ───────────────────────

    private void CallNext()
    {
        foreach (var r in _runs)
        {
            ref var queue = ref _runs.Get2(r);
            if (queue.Waiting) continue;
            if (queue.Index + 1 >= queue.Value.Count) continue;

            queue.Index++;
            queue.Waiting = true;
            queue.Result = string.Empty;
        }
    }

    private void Choose(int index)
    {
        var events = GameConfig.EventsConfig;

        foreach (var r in _runs)
        {
            ref var queue = ref _runs.Get2(r);
            if (!queue.Waiting || queue.Index < 0) continue;

            var definition = events.GetPetition(queue.Value[queue.Index].Id);
            if (definition == null || index < 0 || index >= definition.Choices.Length) continue;

            var choice = definition.Choices[index];
            ref var treasury = ref _runs.Get4(r);
            if (!choice.CanAfford(treasury.Gold, treasury.Food, treasury.Garrison)) continue;

            string result = events.Fill(choice.Result, PlayerName(), LordName(queue.Value[queue.Index].LordId),
                definition.Petitioner);

            ref var request = ref _world.NewEntity().Get<ApplyChoiceEvent>();
            request.Choice = choice;
            request.LordId = queue.Value[queue.Index].LordId;
            request.Result = result;

            queue.Waiting = false;
            queue.Result = result;
        }
    }

    // ─────────────────────── экран ───────────────────────

    private void Repaint()
    {
        var events = GameConfig.EventsConfig;

        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var queue = ref _runs.Get2(r);
            ref var calendar = ref _runs.Get3(r);
            ref var treasury = ref _runs.Get4(r);

            if (calendar.Phase != DayPhase.Morning) continue;

            bool anyLeft = queue.Index + 1 < queue.Value.Count;
            if (anyLeft || queue.Waiting) entity.Get<PhaseLockFlag>();
            else if (entity.Has<PhaseLockFlag>()) entity.Del<PhaseLockFlag>();

            UI.Court.SetQueue(queue.Value.Count == 0
                ? string.Empty
                : $"Проситель {UnityEngine.Mathf.Max(0, queue.Index + 1)} из {queue.Value.Count}");

            UI.Court.SetResult(queue.Result);

            if (!queue.Waiting)
            {
                UI.Court.SetChoices(null, 0, 0, 0, 0);
                UI.Court.SetNext(anyLeft, queue.Index < 0 ? "Звать первого" : "Следующий");

                if (queue.Index < 0)
                    UI.Court.ShowPetition("Тронный зал", "Двери открыты. За ними ждут.");

                continue;
            }

            var entry = queue.Value[queue.Index];
            var definition = events.GetPetition(entry.Id);
            if (definition == null) continue;

            UI.Court.SetNext(false, string.Empty);
            UI.Court.ShowPetition(definition.Petitioner,
                events.Fill(definition.Text, PlayerName(), LordName(entry.LordId), definition.Petitioner));
            UI.Court.SetChoices(definition.Choices, definition.Choices.Length,
                treasury.Gold, treasury.Food, treasury.Garrison);
        }
    }

    // ─────────────────────── мелочи ───────────────────────

    private string PlayerName()
    {
        foreach (var p in _players) return _players.Get2(p).FullName;
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

    private void Subscribe(bool on)
    {
        if (UI.Court.NextButton != null)
        {
            if (on) UI.Court.NextButton.onClick.AddListener(RequestNext);
            else UI.Court.NextButton.onClick.RemoveListener(RequestNext);
        }

        var buttons = UI.Court.Choices;
        if (buttons == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            if (on)
            {
                int index = i;   // локальная копия, иначе все кнопки шлют последний индекс
                buttons[i].onClick.AddListener(() => Raise(index));
            }
            else
            {
                buttons[i].onClick.RemoveAllListeners();
            }
        }
    }

    private void RequestNext() => _world.NewEntity().Get<CallNextPetitionerEvent>();
    private void Raise(int index) => _world.NewEntity().Get<PetitionChoiceEvent>().Index = index;
}
using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Замок. Пока обе булавки — заглушки: стройка ничего не строит, сборы только
/// записываются на завтрашний вечер. Но действие они тратят по-настоящему,
/// поэтому цикл дня уже можно щупать целиком.
/// </summary>
public class CastleActionSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<CastleSlotClickedEvent> _clicks;
    private EcsFilter<SpendActionEvent> _spends;
    private EcsFilter<RunFlag, CalendarAttribute, PlanAttribute> _runs;

    public void Init() => Subscribe(true);
    public void Destroy() => Subscribe(false);

    public void Run()
    {
        foreach (var i in _clicks) Use(_clicks.Get1(i).Slot);
        foreach (var i in _spends) Spend(_spends.Get1(i).Amount);

        RefreshSlots();
    }

    private void Use(CastleSlotId slot)
    {
        foreach (var r in _runs)
        {
            ref var calendar = ref _runs.Get2(r);
            if (calendar.Phase != DayPhase.Day) return;
            if (calendar.ActionsLeft <= 0) return;

            calendar.ActionsLeft--;

            if (slot == CastleSlotId.Gathering)
            {
                ref var plan = ref _runs.Get3(r);
                plan.HasPlan = true;
                plan.Slot = slot;
                plan.PlannedOnDay = calendar.Day;
                Debug.Log($"День {calendar.Day}: пир назначен на завтрашний вечер");
            }
            else
            {
                Debug.Log($"День {calendar.Day}: стройка (заглушка, этап 6)");
            }
        }
    }

    private void Spend(int amount)
    {
        foreach (var r in _runs)
        {
            ref var calendar = ref _runs.Get2(r);
            calendar.ActionsLeft = Mathf.Max(0, calendar.ActionsLeft - amount);
        }
    }

    private void RefreshSlots()
    {
        var slots = UI.CastleSlots;
        if (slots == null) return;

        foreach (var r in _runs)
        {
            ref var calendar = ref _runs.Get2(r);
            bool available = calendar.Phase == DayPhase.Day && calendar.ActionsLeft > 0;

            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) slots[i].SetAvailable(available);
        }
    }

    private void Subscribe(bool on)
    {
        var slots = UI.CastleSlots;
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            var view = slots[i];
            if (view == null || view.Button == null) continue;

            if (on)
            {
                var slot = view.Slot;   // локальная копия, иначе все кнопки нажмут последний слот
                view.Button.onClick.AddListener(() => Raise(slot));
            }
            else
            {
                view.Button.onClick.RemoveAllListeners();
            }
        }
    }

    private void Raise(CastleSlotId slot) =>
        _world.NewEntity().Get<CastleSlotClickedEvent>().Slot = slot;
}
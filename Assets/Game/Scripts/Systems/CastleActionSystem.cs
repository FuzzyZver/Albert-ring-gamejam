using Leopotam.Ecs;

/// <summary>
/// Булавка действий: пир, служба, вербовка. Сами эффекты не применяет —
/// шлёт ApplyChoiceEvent тем же типом ChoiceDefinition, что у просителей
/// и вечерних событий. Своё здесь только три вещи: кулдаун, требование постройки
/// и разовый бонус к обороне.
/// </summary>
public class CastleActionSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<CastleActionsPinClickedEvent> _clicks;
    private EcsFilter<CastleActionRequestEvent> _requests;
    private EcsFilter<CloseCastleCardEvent> _closes;

    private EcsFilter<RunFlag, CalendarAttribute, TreasuryAttribute, CastleHistoryAttribute> _runs;
    private EcsFilter<BuildingAttribute> _buildings;

    private bool _open;

    public void Init() => Subscribe(true);
    public void Destroy() => Subscribe(false);

    public void Run()
    {
        foreach (var _ in _clicks) Open();
        foreach (var _ in _closes) Close();
        foreach (var i in _requests) Perform(_requests.Get1(i).Id);

        RefreshPin();
        if (_open) RefreshCard();
    }

    // ─────────────────────── проведение ───────────────────────

    private void Perform(CastleActionId id)
    {
        var definition = GameConfig.BuildingsConfig.GetAction(id);
        if (definition == null) return;

        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            ref var calendar = ref _runs.Get2(r);
            ref var treasury = ref _runs.Get3(r);
            ref var history = ref _runs.Get4(r);

            if (!Available(definition, calendar, treasury, history, out _)) continue;

            calendar.ActionsLeft--;
            history.Value.Add(new CastleActionUse { Id = id, Day = calendar.Day });

            if (definition.SiegeBonus != 0)
                entity.Get<SiegeBonusAttribute>().Value += definition.SiegeBonus;

            if (definition.QueuesFeast)
            {
                ref var plan = ref entity.Get<PlanAttribute>();
                plan.HasPlan = true;
                plan.PlannedOnDay = calendar.Day;
            }

            ref var request = ref _world.NewEntity().Get<ApplyChoiceEvent>();
            request.Choice = definition.Effect;
            request.LordId = -1;
            request.Result = definition.Effect != null ? definition.Effect.Result : string.Empty;
        }
    }

    /// <summary>Одно место, где решается «можно ли»: и кнопка, и применение спрашивают его.</summary>
    private bool Available(CastleActionDefinition definition, CalendarAttribute calendar,
        TreasuryAttribute treasury, CastleHistoryAttribute history, out string reason)
    {
        if (calendar.Phase != DayPhase.Day) { reason = "не сегодня"; return false; }
        if (calendar.ActionsLeft <= 0) { reason = "действий не осталось"; return false; }

        if (definition.NeedsBuilding && LevelOf(definition.RequiredBuilding) < definition.RequiredLevel)
        {
            var building = GameConfig.BuildingsConfig.GetBuilding(definition.RequiredBuilding);
            reason = "нужно: " + (building != null ? building.TitleAt(definition.RequiredLevel) : "постройка");
            return false;
        }

        if (definition.CooldownDays > 0)
        {
            int last = LastDay(history, definition.Id);
            if (calendar.Day - last < definition.CooldownDays) { reason = "уже устраивал"; return false; }
        }

        var effect = definition.Effect;
        if (effect != null && !effect.CanAfford(treasury.Gold, treasury.Food, treasury.Garrison))
        {
            reason = effect.Missing(treasury.Gold, treasury.Food, treasury.Garrison);
            return false;
        }

        reason = effect != null ? effect.Hint() : string.Empty;
        return true;
    }

    private int LevelOf(BuildingId id)
    {
        foreach (var b in _buildings)
            if (_buildings.Get1(b).Id == id) return _buildings.Get1(b).Level;

        return 0;
    }

    private static int LastDay(CastleHistoryAttribute history, CastleActionId id)
    {
        int last = int.MinValue;
        if (history.Value == null) return last;

        for (int i = 0; i < history.Value.Count; i++)
            if (history.Value[i].Id == id && history.Value[i].Day > last) last = history.Value[i].Day;

        return last;
    }

    // ─────────────────────── экран ───────────────────────

    private void Open()
    {
        _open = true;
        UI.BuildingCard.SetVisible(false);
        UI.CastleActions.SetVisible(true);
    }

    private void Close()
    {
        _open = false;
        UI.CastleActions.SetVisible(false);
    }

    private void RefreshPin()
    {
        var pin = SceneData.ActionsPin;
        if (pin != null) pin.SetLabel("Распорядиться");
    }

    private void RefreshCard()
    {
        var actions = GameConfig.BuildingsConfig.Actions;

        foreach (var r in _runs)
        {
            ref var calendar = ref _runs.Get2(r);
            ref var treasury = ref _runs.Get3(r);
            ref var history = ref _runs.Get4(r);

            for (int i = 0; i < actions.Length; i++)
            {
                var definition = actions[i];
                bool available = Available(definition, calendar, treasury, history, out string reason);

                UI.CastleActions.SetAction(i, definition.Title, definition.Description, reason, available);
            }

            UI.CastleActions.HideFrom(actions.Length);
        }
    }

    private void Subscribe(bool on)
    {
        if (UI.CastleActions.CloseButton != null)
        {
            if (on) UI.CastleActions.CloseButton.onClick.AddListener(RequestClose);
            else UI.CastleActions.CloseButton.onClick.RemoveListener(RequestClose);
        }

        var buttons = UI.CastleActions.Actions;
        if (buttons == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            if (on)
            {
                int index = i;   // локальная копия, иначе все кнопки шлют последнее действие
                buttons[i].onClick.AddListener(() => RequestAction(index));
            }
            else
            {
                buttons[i].onClick.RemoveAllListeners();
            }
        }
    }

    private void RequestAction(int index)
    {
        var actions = GameConfig.BuildingsConfig.Actions;
        if (index < 0 || index >= actions.Length) return;

        _world.NewEntity().Get<CastleActionRequestEvent>().Id = actions[index].Id;
    }

    private void RequestClose() => _world.NewEntity().Get<CloseCastleCardEvent>();
}
using Leopotam.Ecs;

/// <summary>
/// Рисует вечерний экран. Отдельно от EveningSystem потому, что тело поединка
/// дописывает DuelSystem, а она стоит в конвейере позже — рисовать надо после неё.
/// </summary>
public class EveningViewSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<RunFlag, EveningAttribute, CalendarAttribute, TreasuryAttribute> _runs;

    public void Init() => Subscribe(true);
    public void Destroy() => Subscribe(false);

    public void Run()
    {
        var events = GameConfig.EventsConfig;

        foreach (var r in _runs)
        {
            ref var evening = ref _runs.Get2(r);
            ref var calendar = ref _runs.Get3(r);
            ref var treasury = ref _runs.Get4(r);

            if (calendar.Phase != DayPhase.Evening) continue;

            if (evening.Kind == EveningKind.None)
            {
                UI.Evening.Show("Вечер", "Замок затихает.");
                UI.Evening.SetResult(string.Empty);
                UI.Evening.SetChoices(null, 0, 0, 0, 0);
                continue;
            }

            UI.Evening.Show(evening.Title, evening.Body);
            UI.Evening.SetResult(evening.Result);

            if (evening.ShowingResult)
            {
                UI.Evening.SetSingleChoice("Дальше");
                continue;
            }

            if (!evening.Waiting)
            {
                UI.Evening.SetChoices(null, 0, 0, 0, 0);
                continue;
            }

            if (evening.Kind == EveningKind.Duel)
            {
                UI.Evening.SetSingleChoice("Выйти во двор");
                continue;
            }

            var definition = events.GetEvening(evening.Id);
            if (definition == null) continue;

            if (definition.Choices.Length == 0)
            {
                UI.Evening.SetSingleChoice("Дальше");
                continue;
            }

            UI.Evening.SetChoices(definition.Choices, definition.Choices.Length,
                treasury.Gold, treasury.Food, treasury.Garrison);
        }
    }

    private void Subscribe(bool on)
    {
        var buttons = UI.Evening.Choices;
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

    private void Raise(int index) => _world.NewEntity().Get<EveningChoiceEvent>().Index = index;
}
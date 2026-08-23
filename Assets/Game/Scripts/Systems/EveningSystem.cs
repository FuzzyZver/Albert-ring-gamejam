using Leopotam.Ecs;

/// <summary>
/// Вечерний экран. Сам событий не выдумывает — только рисует то, что положили
/// в EveningAttribute, и превращает нажатие в EveningChoiceEvent.
/// Когда появятся пиры, молебны и доносы, добавлять придётся не сюда,
/// а системы, которые заполняют этот компонент.
/// </summary>
public class EveningSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<PhaseChangedEvent> _phaseStarts;
    private EcsFilter<PhaseEndedEvent> _phaseEnds;
    private EcsFilter<EveningChoiceEvent> _choices;
    private EcsFilter<RunFlag, EveningAttribute> _runs;

    private readonly string[] _labels = new string[1];

    public void Init() => Subscribe(true);
    public void Destroy() => Subscribe(false);

    public void Run()
    {
        foreach (var i in _phaseStarts)
            if (_phaseStarts.Get1(i).Phase == DayPhase.Evening) Open();

        foreach (var i in _phaseEnds)
            if (_phaseEnds.Get1(i).Phase == DayPhase.Evening) Close();

        foreach (var i in _choices) Choose(_choices.Get1(i).Index);

        Repaint();
    }

    /// <summary>Если к этому моменту никто вечер не занял — значит он спокойный.</summary>
    private void Open()
    {
        foreach (var r in _runs)
        {
            ref var evening = ref _runs.Get2(r);
            if (evening.Kind != EveningKind.None) continue;

            evening.Kind = EveningKind.Message;
            evening.Title = "Вечер";
            evening.Body = GameConfig.BalanceConfig.EveningQuietText;
            evening.Choice = string.Empty;
            evening.Waiting = false;
        }
    }

    private void Close()
    {
        foreach (var r in _runs)
        {
            ref var evening = ref _runs.Get2(r);
            evening.Kind = EveningKind.None;
            evening.LordId = -1;
            evening.Waiting = false;
        }
    }

    private void Choose(int index)
    {
        foreach (var r in _runs)
        {
            ref var evening = ref _runs.Get2(r);
            if (!evening.Waiting) continue;

            if (evening.Kind == EveningKind.Duel && index == 0)
                _runs.GetEntity(r).Get<DuelAcceptedFlag>();
        }
    }

    private void Repaint()
    {
        foreach (var r in _runs)
        {
            ref var evening = ref _runs.Get2(r);
            if (evening.Kind == EveningKind.None) continue;

            UI.Evening.Show(evening.Title, evening.Body);

            if (string.IsNullOrEmpty(evening.Choice))
            {
                UI.Evening.SetChoices(_labels, 0);
                continue;
            }

            _labels[0] = evening.Choice;
            UI.Evening.SetChoices(_labels, 1);
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
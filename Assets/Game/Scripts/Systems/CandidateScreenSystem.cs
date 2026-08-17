using Leopotam.Ecs;

/// <summary>
/// Окно выбора персонажа. Кнопки подписываются один раз в Init:
/// сами карточки живут всю сессию, меняется только содержимое.
/// </summary>
public class CandidateScreenSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<CourtReadyEvent> _courtReady;
    private EcsFilter<RunReadyEvent> _runReady;
    private EcsFilter<RunFlag, CourtAttribute> _runs;

    public void Init()
    {
        var cards = UI.CharacterSelect.Cards;
        if (cards == null) return;

        for (int i = 0; i < cards.Length; i++)
        {
            int index = i;   // без локальной копии все кнопки выберут последнего
            cards[i].Button.onClick.AddListener(() => Choose(index));
        }
    }

    public void Run()
    {
        foreach (var _ in _courtReady) Show();
    }

    public void Destroy()
    {
        var cards = UI.CharacterSelect.Cards;
        if (cards == null) return;

        for (int i = 0; i < cards.Length; i++)
            cards[i].Button.onClick.RemoveAllListeners();
    }

    private void Show()
    {
        var chars = GameConfig.CharactersConfig;
        var cards = UI.CharacterSelect.Cards;

        UI.CharacterSelect.SetVisible(true);
        UI.CharacterSelect.HideAllCards();

        foreach (var i in _runs)
        {
            var candidates = _runs.Get2(i).Value.Candidates;

            for (int c = 0; c < candidates.Count && c < cards.Length; c++)
            {
                var person = candidates[c];
                cards[c].Set(
                    person.FullName,
                    chars.TraitLine(person.TraitA, person.TraitB, person.Gender),
                    chars.TraitHints(person.TraitA, person.TraitB));
            }
        }
    }

    private void Choose(int index)
    {
        _world.NewEntity().Get<SelectCandidateEvent>().Index = index;
    }
}
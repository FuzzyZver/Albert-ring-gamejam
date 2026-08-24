using Leopotam.Ecs;

/// <summary>Шапка карточки. Кликов не слушает — смотрит на выделение,
/// которым владеет SelectionSystem.</summary>
public class LordCardSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<RunFlag, SelectionAttribute> _runs;
    private EcsFilter<LordIdAttribute, PersonAttribute, TraitsAttribute> _people;
    private EcsFilter<LordFlag, LordIdAttribute, PersonAttribute> _lords;

    public void Init()
    {
        if (UI.LordCard.CloseButton != null)
            UI.LordCard.CloseButton.onClick.AddListener(RequestClose);
    }

    public void Destroy()
    {
        if (UI.LordCard.CloseButton != null)
            UI.LordCard.CloseButton.onClick.RemoveListener(RequestClose);
    }

    public void Run()
    {
        foreach (var r in _runs)
        {
            if (!_runs.GetEntity(r).Has<SelectionChangedFlag>()) continue;

            ref var selection = ref _runs.Get2(r);
            if (!selection.HasTarget || !TryFind(selection.LordId, out var target))
            {
                UI.LordCard.SetVisible(false);
                continue;
            }

            Show(target);
        }
    }

    private void Show(EcsEntity entity)
    {
        var chars = GameConfig.CharactersConfig;
        var balance = GameConfig.BalanceConfig;

        ref var person = ref entity.Get<PersonAttribute>();
        ref var traits = ref entity.Get<TraitsAttribute>();
        string traitLine = chars.TraitLine(traits.A, traits.B, person.Gender);

        if (!entity.Has<LordFlag>())
        {
            UI.LordCard.ShowPlayer(person.FullName, traitLine);
            return;
        }

        int opinion = entity.Get<OpinionAttribute>().Value;

        var ambition = chars.GetAmbition(entity.Get<AmbitionAttribute>().Id);

        UI.LordCard.ShowLord(
            person.FullName,
            traitLine,
            ambition != null ? ambition.Title : string.Empty,
            ambition != null ? ambition.Demand : string.Empty,
            RivalLine(entity.Get<RivalAttribute>().LordId),
            opinion,
            entity.Get<TroopsAttribute>().Value,
            opinion >= balance.TroopsComeAtOpinion);
    }

    private bool TryFind(int lordId, out EcsEntity found)
    {
        foreach (var i in _people)
        {
            if (_people.Get1(i).Value != lordId) continue;
            found = _people.GetEntity(i);
            return true;
        }

        found = default;
        return false;
    }

    private string RivalLine(int lordId)
    {
        if (lordId < 0) return "врагов нет";

        foreach (var i in _lords)
            if (_lords.Get2(i).Value == lordId)
                return "враг " + _lords.Get3(i).GivenName;

        return "врагов нет";
    }

    private void RequestClose() => _world.NewEntity().Get<CloseCardEvent>();
}
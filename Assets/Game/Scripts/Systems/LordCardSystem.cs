using Leopotam.Ecs;
using UnityEngine;

/// <summary>Клик по булавке -> карточка персонажа. Игроку показывается урезанная версия.</summary>
public class LordCardSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{

    private EcsFilter<PinClickedEvent> _clicks;
    private EcsFilter<CloseCardEvent> _closes;
    private EcsFilter<CourtReadyEvent> _courtReady;
    private EcsFilter<LordIdAttribute, PersonAttribute>.Exclude<PlayerFlag> _lords;

    public void Init()
    {
        if (UI.LordCard.CloseButton != null)
            UI.LordCard.CloseButton.onClick.AddListener(RequestClose);
    }

    public void Run()
    {
        foreach (var _ in _courtReady) UI.LordCard.SetVisible(false);
        foreach (var _ in _closes) UI.LordCard.SetVisible(false);

        foreach (var i in _clicks)
        {
            var target = _clicks.Get1(i).Target;
            if (target.IsAlive()) Show(target);
        }
    }

    public void Destroy()
    {
        if (UI.LordCard.CloseButton != null)
            UI.LordCard.CloseButton.onClick.RemoveListener(RequestClose);
    }

    private void Show(EcsEntity entity)
    {
        var chars = GameConfig.CharactersConfig;

        ref var person = ref entity.Get<PersonAttribute>();
        ref var traits = ref entity.Get<TraitsAttribute>();
        string traitLine = chars.TraitLine(traits.A, traits.B, person.Gender);

        if (!entity.Has<LordFlag>())
        {
            UI.LordCard.ShowPlayer(person.FullName, traitLine);
            return;
        }

        UI.LordCard.ShowLord(
            person.FullName,
            traitLine,
            chars.AmbitionTitle(entity.Get<AmbitionAttribute>().Id),
            RivalName(entity.Get<RivalAttribute>().LordId),
            entity.Get<OpinionAttribute>().Value,
            entity.Get<TroopsAttribute>().Value);
    }

    private string RivalName(int lordId)
    {
        if (lordId < 0) return "нет";

        foreach (var i in _lords)
            if (_lords.Get1(i).Value == lordId)
                return _lords.Get2(i).FullName;

        return "нет";
    }

    private void RequestClose()
    {
        EcsWorld.NewEntity().Get<CloseCardEvent>();
    }
}
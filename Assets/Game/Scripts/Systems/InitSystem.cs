using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Всё, что делает старт игры: гасит окна и просит первый забег.
/// Сама генерация живёт в RunSetupSystem, чтобы рестарт шёл ровно тем же путём,
/// а не отдельной копией кода.
/// </summary>
public class InitSystem : Injects, IEcsInitSystem
{
    public void Init()
    {
        RealtimeData.Load();   // открытые концовки переживают забеги

        UI.Screens.Show(ScreenId.None);
        UI.CharacterSelect.SetVisible(false);
        UI.LordCard.SetVisible(false);
        UI.Epilogue.SetVisible(false);
        UI.BuildingCard.SetVisible(false);
        UI.CastleActions.SetVisible(false);
        UI.Battle.SetVisible(false);
        UI.Hud.SetVisible(false);

        EcsWorld.NewEntity().Get<NewRunEvent>().Seed = GameConfig.BalanceConfig.NextSeed();
    }
}
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
        UI.CharacterSelect.SetVisible(false);
        UI.LordCard.SetVisible(false);
        //if (UI.MapRoot != null) UI.MapRoot.SetActive(false);

        EcsWorld.NewEntity().Get<NewRunEvent>().Seed = GameConfig.BalanceConfig.NextSeed();
    }
}
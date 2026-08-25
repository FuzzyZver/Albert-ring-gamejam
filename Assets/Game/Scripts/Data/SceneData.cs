using UnityEngine;

/// <summary>
/// Точка доступа к объектам сцены из систем. Если у тебя тут уже есть свои поля —
/// не заменяй файл, а допиши недостающие.
/// </summary>
public class SceneData : MonoBehaviour
{
    [Header("Булавки на карте")]
    public LordActor PlayerCastle;   // замок игрока: тот же LordActor, отличается Id = -1
    public LordActor[] LordPins;     // по числу лордов из BalanceConfig.LordsCount

    [Header("Булавки в замке")]
    public BuildingActor[] BuildingPins;   // по одной на BuildingId
    public CastleActionsActor ActionsPin;
}
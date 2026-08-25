using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfig")]
public class GameConfig : ScriptableObject
{
    public CharactersConfig CharactersConfig;
    public BalanceConfig BalanceConfig;
    public EventsConfig EventsConfig;
    public BuildingsConfig BuildingsConfig;
}

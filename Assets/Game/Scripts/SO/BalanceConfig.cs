using UnityEngine;

[CreateAssetMenu(fileName = "BalanceConfig", menuName = "Configs/BalanceConfig")]
public class BalanceConfig : ScriptableObject
{
    [Header("Забег")]
    public int DaysUntilSiege = 12;
    public int LordsCount = 5;
    public int ActionsPerDay = 2;
    public int PetitionersPerMorning = 3;
    public int SiegeStrength = 110;

    [Header("Старт")]
    public int StartGold = 40;
    public int StartFood = 60;
    public int StartGarrison = 10;
    public int StartLordOpinion = 0;
    public int StartCommonsOpinion = 0;

    [Header("Лорды")]
    public int LordTroopsMin = 15;
    public int LordTroopsMax = 35;
    public int TroopsComeAtOpinion = 30;   // придёт на осаду при мнении выше
    public int LoverComeAtOpinion = 0;     // любовник приходит раньше
    [Range(0, 100)] public int EpithetChance = 40;

    [Header("Смерти")]
    public int RiotBelowCommons = -50;
    public int AssassinationBelowOpinion = -60;
    public int OverthrowBelowOpinion = -30;
    public int OverthrowLordsCount = 3;
    public int FamineDaysToRiot = 2;

    [Header("Налоги, индекс = ползунок 0..3")]
    public int[] PeasantTaxFood = { 0, 6, 12, 18 };
    public int[] PeasantTaxOpinion = { 5, 0, -8, -18 };
    public int[] LordTaxGold = { 0, 8, 16, 24 };
    public int[] LordTaxOpinion = { 3, 0, -6, -14 };

    [Header("Прочее")]
    public int TroopsPerFood = 10;   // гарнизон ест: 1 пища за каждые 10 копий
    public int WallsStrength = 15;
    public int GranaryStrength = 10;

    public int PeasantFood(int slider) => Clamped(PeasantTaxFood, slider);
    public int PeasantOpinion(int slider) => Clamped(PeasantTaxOpinion, slider);
    public int LordGold(int slider) => Clamped(LordTaxGold, slider);
    public int LordOpinion(int slider) => Clamped(LordTaxOpinion, slider);

    private static int Clamped(int[] table, int slider)
    {
        if (table == null || table.Length == 0) return 0;
        return table[Mathf.Clamp(slider, 0, table.Length - 1)];
    }
}
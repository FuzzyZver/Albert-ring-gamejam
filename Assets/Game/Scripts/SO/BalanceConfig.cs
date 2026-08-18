using UnityEngine;

[CreateAssetMenu(fileName = "BalanceConfig", menuName = "Configs/BalanceConfig")]
public class BalanceConfig : ScriptableObject
{
    [Header("Забег")]
    public int DaysUntilSiege = 12;
    public int LordsCount = 5;
    public int CandidatesCount = 3;   // из скольких выбираешь себя
    public int ActionsPerDay = 2;
    public int PetitionersPerMorning = 3;
    public int SiegeStrength = 110;

    [Header("Старт")]
    public int StartGold = 30;
    public int StartFood = 40;
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
    public int[] PeasantTaxFood = { 0, 4, 8, 12 };
    public int[] PeasantTaxOpinion = { 6, 0, -8, -18 };
    public int[] LordTaxGold = { 0, 6, 12, 18 };
    public int[] LordTaxOpinion = { 3, 0, -6, -14 };

    [Header("Содержание")]
    public int TroopsPerFood = 2;    // одна пища кормит столько копий
    public int StarvingCommonsPenalty = -10;
    public int OpinionMin = -100;
    public int OpinionMax = 100;

    [Header("Осада")]
    public int WallsStrength = 15;
    public int GranaryStrength = 10;

    [Header("Тексты")]
    public string[] PhaseNames = { "Утро", "День", "Вечер", "Ночь" };
    public string[] PhaseButtons = { "Распустить двор", "Вечереет", "Ночь", "Спать" };

    [Header("Отладка")]
    public int FixedSeed = 0;   // 0 = каждый забег новый

    /// <summary>Сид следующего забега. Поставь FixedSeed, чтобы ловить баг на одном дворе.</summary>
    public int NextSeed() => FixedSeed != 0 ? FixedSeed : System.Environment.TickCount;

    public int PeasantFood(int slider) => Clamped(PeasantTaxFood, slider);
    public int PeasantOpinion(int slider) => Clamped(PeasantTaxOpinion, slider);
    public int LordGold(int slider) => Clamped(LordTaxGold, slider);
    public int LordOpinion(int slider) => Clamped(LordTaxOpinion, slider);

    public string PhaseName(DayPhase phase) => Text(PhaseNames, (int)phase, phase.ToString());
    public string PhaseButton(DayPhase phase) => Text(PhaseButtons, (int)phase, "Дальше");

    /// <summary>Сколько пищи съедает гарнизон за ночь. Одно копьё всё равно ест.</summary>
    public int FoodUpkeep(int garrison) =>
        garrison <= 0 ? 0 : Mathf.CeilToInt(garrison / (float)Mathf.Max(1, TroopsPerFood));

    public int ClampOpinion(int value) => Mathf.Clamp(value, OpinionMin, OpinionMax);

    private static int Clamped(int[] table, int slider)
    {
        if (table == null || table.Length == 0) return 0;
        return table[Mathf.Clamp(slider, 0, table.Length - 1)];
    }

    private static string Text(string[] table, int index, string fallback)
    {
        if (table == null || index < 0 || index >= table.Length) return fallback;
        return table[index];
    }
}
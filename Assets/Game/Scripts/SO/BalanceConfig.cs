using UnityEngine;

[CreateAssetMenu(fileName = "BalanceConfig", menuName = "Configs/BalanceConfig")]
public class BalanceConfig : ScriptableObject
{
    [Header("Сложность")]
    [Range(0, 5)] public int Difficulty = 1;   // 0 = лёгкая, 1 = нормальная, 2 = жёсткая

    [Header("Забег")]
    public int DaysUntilSiege = 12;
    public int LordsCount = 5;
    public int CandidatesCount = 3;   // из скольких выбираешь себя
    public int ActionsPerDay = 2;
    public int PetitionersPerMorning = 3;

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
    public int RiotWarningMargin = 15;   // за сколько до бунта предупреждать
    public int AssassinationBelowOpinion = -60;
    [Range(0, 100)] public int AssassinationChance = 25;
    [Range(0, 100)] public int VengefulAssassinationChance = 8;   // затаившему обиду хватает и меньшего
    public int OverthrowBelowOpinion = -30;
    public int OverthrowLordsCount = 3;
    public int FamineNightsToDeath = 3;

    [Header("Поединок")]
    [Range(0, 100)] public int DuelWinChanceBase = 55;   // ТВОЙ шанс победить, не шанс получить вызов
    public int DuelChanceMin = 5;
    public int DuelChanceMax = 95;

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
    public int SiegeEnemyForce = 95;

    /// <summary>Модификатор по числу приехавших лордов: без них плохо, впятером страшно.
    /// Лорды приезжают командирами, войска остаются при их замках.</summary>
    public int[] LordCountBonus = { -15, 0, 8, 18, 30, 45 };

    public int FoodComfort = 30;        // запас, который считается хорошим
    public int FoodBonusMax = 15;
    public int FoodPenaltyMax = -20;
    public int CommonsDivider = 4;      // мнение крестьян делится на это

    [Header("Осада: стычки")]
    public float BattleSpawnInterval = 2f;
    public float BattleDuration = 4f;
    public int SquadMin = 6;
    public int SquadMax = 13;
    public int MoraleToSquad = 12;      // сколько морали даёт +1 к отряду
    public int MoraleToRoll = 8;        // сколько морали даёт +1 к кубику
    public int LordCommanderBonus = 4;
    public int PlayerCommanderBonus = 6;
    public int EnemyRoutPercent = 25;
    public int OurRoutPercent = 25;
    public int MoraleToRoutRelief = 3;  // сколько морали снижает порог бегства на 1

    [Header("Просьба о войске")]
    [Range(0, 100)] public int TroopsPercentOnRequest = 50;   // сколько копий лорд отдаёт сразу
    public int TroopsChanceBase = 20;        // шанс согласия при мнении 0
    public int TroopsChancePerOpinion = 2;   // за каждое очко мнения
    public int TroopsChanceMax = 95;

    [Header("Память крестьян")]
    public int TaxNeutralLevel = 1;   // выше этого уровня копится злость
    public int GrudgePerLevel = 2;    // очков мнения за каждую единицу злости
    public int GrudgeDecay = 1;       // сколько сходит за спокойную ночь
    public int GrudgeMax = 8;

    [Header("Отладка")]
    public int FixedSeed = 0;   // 0 = каждый забег новый

    /// <summary>Сид следующего забега. Поставь FixedSeed, чтобы ловить баг на одном дворе.</summary>
    public int NextSeed() => FixedSeed != 0 ? FixedSeed : System.Environment.TickCount;

    public int PeasantFood(int slider) => Clamped(PeasantTaxFood, slider);
    public int PeasantOpinion(int slider) => Clamped(PeasantTaxOpinion, slider);
    public int LordGold(int slider) => Clamped(LordTaxGold, slider);
    public int LordOpinion(int slider) => Clamped(LordTaxOpinion, slider);

    /// <summary>Сколько пищи съедает гарнизон за ночь. Одно копьё всё равно ест.</summary>
    public int FoodUpkeep(int garrison) =>
        garrison <= 0 ? 0 : Mathf.CeilToInt(garrison / (float)Mathf.Max(1, TroopsPerFood));

    public int ClampOpinion(int value) => Mathf.Clamp(value, OpinionMin, OpinionMax);

    /// <summary>Шанс, что лорд отдаст копья. При мнении +30 уже прилично, дальше растёт.</summary>
    public int TroopsChance(int opinion) =>
        Mathf.Clamp(TroopsChanceBase + opinion * TroopsChancePerOpinion, 0, TroopsChanceMax);

    private static int Clamped(int[] table, int slider)
    {
        if (table == null || table.Length == 0) return 0;
        return table[Mathf.Clamp(slider, 0, table.Length - 1)];
    }

}
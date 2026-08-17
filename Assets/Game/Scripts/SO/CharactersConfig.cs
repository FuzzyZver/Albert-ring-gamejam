using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactersConfig", menuName = "Configs/CharactersConfig")]
public class CharactersConfig : ScriptableObject
{
    // ─────────────────────────────  ПУЛЫ ИМЁН  ─────────────────────────────

    public string[] MaleTitles = { "Сир", "Лорд", "Барон", "Брат" };
    public string[] FemaleTitles = { "Леди", "Госпожа", "Сестра" };

    public string[] MaleNames = { "Айвор", "Одо", "Гэл", "Ренар", "Бертран",
                                  "Хальд", "Кутберт", "Вальд", "Ансельм", "Тибо" };
    public string[] FemaleNames = { "Мора", "Иделия", "Санча", "Ровена", "Берта",
                                    "Аделина", "Юта", "Мелисента" };

    public string[] MaleEpithets = { "Хромой", "Рыжий", "Тихий", "Долговязый",
                                     "Кривой", "Праведный", "Неудачливый", "Сытый" };
    public string[] FemaleEpithets = { "Хромая", "Рыжая", "Тихая", "Долговязая",
                                       "Кривая", "Праведная", "Неудачливая", "Сытая" };

    // ─────────────────────────────  ДАННЫЕ  ─────────────────────────────

    public TraitDefinition[] Traits = DefaultTraits();
    public VerbDefinition[] Verbs = DefaultVerbs();
    public AmbitionDefinition[] Ambitions = DefaultAmbitions();
    public ConsequenceDefinition[] Consequences = DefaultConsequences();

    public TraitDefinition GetTrait(TraitId id) => Array.Find(Traits, t => t.Id == id);
    public VerbDefinition GetVerb(VerbId id) => Array.Find(Verbs, v => v.Id == id);
    public AmbitionDefinition GetAmbition(AmbitionId id) => Array.Find(Ambitions, a => a.Id == id);
    public ConsequenceDefinition GetConsequence(ConsequenceId id) => Array.Find(Consequences, c => c.Id == id);

    public string[] Titles(Gender gender) => gender == Gender.Male ? MaleTitles : FemaleTitles;
    public string[] Names(Gender gender) => gender == Gender.Male ? MaleNames : FemaleNames;
    public string[] Epithets(Gender gender) => gender == Gender.Male ? MaleEpithets : FemaleEpithets;

    // ─────────────────────  СТАРТОВАЯ МАТРИЦА  ─────────────────────
    // Заполняется один раз при создании ассета. Дальше правишь в инспекторе,
    // код сюда больше не лезет. Матрица РАЗРЕЖЕННАЯ: черта реагирует
    // на два-три глагола, на остальные молчит.

    private static TraitDefinition[] DefaultTraits() => new[]
    {
        new TraitDefinition {
            Id = TraitId.Proud, Title = "Гордый", TitleFemale = "Гордая",
            Hint = "Не продаётся. Помнит оскорбления.",
            Reactions = new[] {
                R(VerbId.Flatter,  15,  0, ConsequenceId.None,            "любит лесть"),
                R(VerbId.Bribe,   -30,  0, ConsequenceId.None,            "не продаётся"),
                R(VerbId.Threaten,-25,  0, ConsequenceId.ChallengeToDuel, "вызовет на поединок"),
                R(VerbId.Insult,  -20,  0, ConsequenceId.ChallengeToDuel, "вызовет на поединок"),
            },
            SelfReactions = new[] {
                R(VerbId.Threaten,  5, 0, ConsequenceId.None, "ты умеешь пугать"),
                R(VerbId.Bribe,   -10, 0, ConsequenceId.None, "тебе противно платить"),
            },
        },

        new TraitDefinition {
            Id = TraitId.Greedy, Title = "Алчный", TitleFemale = "Алчная",
            Hint = "Всё имеет цену. Особенно он.",
            Reactions = new[] {
                R(VerbId.Bribe,          25, 0, ConsequenceId.None,        "берёт и просит ещё"),
                R(VerbId.Flatter,        -5, 0, ConsequenceId.None,        "словами сыт не будет"),
                R(VerbId.AskForTroops,  -10, 0, ConsequenceId.DemandGift,  "потребует плату"),
                R(VerbId.FulfillAmbition,10, 0, ConsequenceId.None,        ""),
            },
            SelfReactions = new[] {
                R(VerbId.Bribe, 10, 0, ConsequenceId.None, "ты знаешь настоящую цену"),
            },
            GoldPerDay = 2,
        },

        new TraitDefinition {
            Id = TraitId.Cunning, Title = "Хитрый", TitleFemale = "Хитрая",
            Hint = "Видит лесть насквозь. Доносит.",
            Reactions = new[] {
                R(VerbId.Flatter, -15, 0, ConsequenceId.None,       "видит насквозь"),
                R(VerbId.Bribe,     5, 0, ConsequenceId.TellRival,  "возьмёт и расскажет"),
                R(VerbId.Threaten, -5, 0, ConsequenceId.SpreadRumor, "пустит слух"),
                R(VerbId.Insult,    5, 0, ConsequenceId.None,       "уважает прямоту"),
            },
            SelfReactions = new[] {
                R(VerbId.Flatter, 5, 0, ConsequenceId.None, ""),
                R(VerbId.Insult,  5, 0, ConsequenceId.None, ""),
            },
        },

        new TraitDefinition {
            Id = TraitId.Pious, Title = "Набожный", TitleFemale = "Набожная",
            Hint = "Судит тебя. Вслух.",
            Reactions = new[] {
                R(VerbId.Seduce,          0, -25, ConsequenceId.Scandal,           "провал = скандал"),
                R(VerbId.DrinkTogether, -10,   0, ConsequenceId.None,              "не пьёт"),
                R(VerbId.Insult,        -15,   0, ConsequenceId.PublicRepentance,  "покается прилюдно"),
                R(VerbId.Flatter,         5,   0, ConsequenceId.None,              ""),
            },
            SelfReactions = new[] {
                R(VerbId.Seduce, 0, -10, ConsequenceId.None, "тебе стыдно"),
            },
            CommonsOpinion = 10,
        },

        new TraitDefinition {
            Id = TraitId.Cruel, Title = "Жестокий", TitleFemale = "Жестокая",
            Hint = "Уважает только силу.",
            Reactions = new[] {
                R(VerbId.Threaten,      15, 0, ConsequenceId.None, "понимает только это"),
                R(VerbId.Flatter,      -10, 0, ConsequenceId.None, "презирает лизоблюдов"),
                R(VerbId.Insult,         5, 0, ConsequenceId.None, ""),
                R(VerbId.AskForTroops,  10, 0, ConsequenceId.None, "любит войну"),
            },
            SelfReactions = new[] {
                R(VerbId.Threaten, 10, 0, ConsequenceId.None, "тебе верят"),
            },
            CommonsOpinion = -10,
        },

        new TraitDefinition {
            Id = TraitId.Craven, Title = "Трусливый", TitleFemale = "Трусливая",
            Hint = "Прогибается. И сбегает.",
            Reactions = new[] {
                R(VerbId.Threaten,      25, 0, ConsequenceId.None,       "сразу сдастся"),
                R(VerbId.AskForTroops, -20, 0, ConsequenceId.LeaveCourt, "уедет от греха"),
                R(VerbId.Insult,       -10, 0, ConsequenceId.LeaveCourt, "уедет от греха"),
            },
            SelfReactions = new[] {
                R(VerbId.Threaten,     -15, 0, ConsequenceId.None, "тебе не верят"),
                R(VerbId.AskForTroops,  -5, 0, ConsequenceId.None, ""),
            },
        },

        new TraitDefinition {
            Id = TraitId.Lustful, Title = "Похотливый", TitleFemale = "Похотливая",
            Hint = "Согласен почти на всё.",
            Reactions = new[] {
                R(VerbId.Seduce,          0, 30, ConsequenceId.None, "почти согласен"),
                R(VerbId.DrinkTogether,  10,  0, ConsequenceId.None, ""),
                R(VerbId.Flatter,        10,  0, ConsequenceId.None, ""),
            },
            SelfReactions = new[] {
                R(VerbId.Seduce, 0, 15, ConsequenceId.None, "ты знаешь, что делаешь"),
            },
            UnlockedVerbs = new[] { VerbId.Seduce },
        },

        new TraitDefinition {
            Id = TraitId.Drunkard, Title = "Пьяница", TitleFemale = "Пьяница",
            Hint = "Лучший друг за столом. Худший — наутро.",
            Reactions = new[] {
                R(VerbId.DrinkTogether, 25, 0, ConsequenceId.GetDrunk, "напьётся"),
                R(VerbId.Bribe,         10, 0, ConsequenceId.None,     ""),
                R(VerbId.Threaten,      -5, 0, ConsequenceId.None,     ""),
            },
            SelfReactions = new[] {
                R(VerbId.DrinkTogether, 15, 0, ConsequenceId.ConfessDrunkenly, "ты расскажешь всё"),
            },
            DailyRisk = ConsequenceId.FallDownStairs, DailyRiskChance = 4,
        },

        new TraitDefinition {
            Id = TraitId.Obsessed, Title = "Одержимый", TitleFemale = "Одержимая",
            Hint = "У него одна мысль. Ты в ней мешаешь.",
            Reactions = new[] {
                R(VerbId.FulfillAmbition, 30, 0, ConsequenceId.None,       "только это и важно"),
                R(VerbId.Flatter,        -10, 0, ConsequenceId.None,       "не слышит"),
                R(VerbId.Bribe,          -10, 0, ConsequenceId.None,       "не слышит"),
                R(VerbId.Insult,         -25, 0, ConsequenceId.PlotMurder, "начнёт готовить убийство"),
            },
            SelfReactions = new[] {
                R(VerbId.FulfillAmbition, 10, 0, ConsequenceId.None, ""),
            },
            DailyRisk = ConsequenceId.ProphecyFulfilled, DailyRiskChance = 3,
        },

        new TraitDefinition {
            Id = TraitId.Honest, Title = "Честный", TitleFemale = "Честная",
            Hint = "Не врёт. Тебе тоже не даст.",
            Reactions = new[] {
                R(VerbId.Bribe,        -25, 0, ConsequenceId.TellRival, "донесёт о взятке"),
                R(VerbId.Flatter,      -10, 0, ConsequenceId.None,      "не любит лести"),
                R(VerbId.Threaten,       5, 0, ConsequenceId.None,      "ценит прямоту"),
                R(VerbId.AskForTroops,  15, 0, ConsequenceId.None,      "слово держит"),
            },
            SelfReactions = new[] {
                R(VerbId.Bribe,        -15, 0, ConsequenceId.None, "у тебя дрожат руки"),
                R(VerbId.Flatter,      -10, 0, ConsequenceId.None, "звучит фальшиво"),
                R(VerbId.AskForTroops,  10, 0, ConsequenceId.None, "тебе верят"),
            },
            CommonsOpinion = 10,
        },
    };

    private static VerbDefinition[] DefaultVerbs() => new[]
    {
        new VerbDefinition {
            Id = VerbId.Flatter, Title = "Польстить", Hint = "Бесплатно и почти всегда работает",
            BaseOpinion = 10, BaseChance = 100,
        },
        new VerbDefinition {
            Id = VerbId.Bribe, Title = "Подкупить", Hint = "Деньги решают. Не всё",
            BaseOpinion = 15, BaseChance = 100, GoldCost = 15,
        },
        new VerbDefinition {
            Id = VerbId.Threaten, Title = "Пригрозить", Hint = "Дёшево. Дорого потом",
            BaseOpinion = -20, BaseChance = 100, RivalOpinion = 5,
        },
        new VerbDefinition {
            Id = VerbId.DrinkTogether, Title = "Выпить вместе", Hint = "Съедает припасы",
            BaseOpinion = 5, BaseChance = 100, FoodCost = 6, CourtOpinion = 2,
        },
        new VerbDefinition {
            Id = VerbId.Seduce, Title = "Соблазнить", Hint = "Провал = скандал на весь двор",
            BaseOpinion = 30, BaseChance = 45, OnFail = ConsequenceId.Scandal,
        },
        new VerbDefinition {
            Id = VerbId.Insult, Title = "Послать", Hint = "Радует соперника и крестьян",
            BaseOpinion = -45, BaseChance = 100, RivalOpinion = 30, CommonsOpinion = 10,
        },
        new VerbDefinition {
            Id = VerbId.FulfillAmbition, Title = "Исполнить желание", Hint = "Один раз на лорда",
            BaseOpinion = 50, BaseChance = 100, OncePerLord = true,
        },
        new VerbDefinition {
            Id = VerbId.AskForTroops, Title = "Просить войск", Hint = "Унизительно, но копья нужны",
            BaseOpinion = -10, BaseChance = 100, OncePerLord = true,
        },
    };

    private static AmbitionDefinition[] DefaultAmbitions() => new[]
    {
        new AmbitionDefinition { Id = AmbitionId.MarryMyDaughter,   Title = "Женись на моей дочери", Demand = "У меня дочь на выданье, государь.", OpinionOnFulfill = 50, ClosesRomance = true, CourtOpinion = -5, OnRefuse = ConsequenceId.SpreadRumor },
        new AmbitionDefinition { Id = AmbitionId.GiveMeTheMill,     Title = "Отдай мельницу",        Demand = "Мельница у брода должна быть моей.", OpinionOnFulfill = 40, CommonsOpinion = -15, OnRefuse = ConsequenceId.DemandGift },
        new AmbitionDefinition { Id = AmbitionId.GrantMeATitle,     Title = "Дай титул",             Demand = "Я достоин большего, чем есть.",     OpinionOnFulfill = 45, CourtOpinion = -10, OnRefuse = ConsequenceId.LeaveCourt },
        new AmbitionDefinition { Id = AmbitionId.KillMyRival,       Title = "Убей моего врага",      Demand = "Ты знаешь, о ком я.",               OpinionOnFulfill = 60, CourtOpinion = -20, OnRefuse = ConsequenceId.PlotMurder },
        new AmbitionDefinition { Id = AmbitionId.BuildTheChapel,    Title = "Построй часовню",       Demand = "Господь смотрит, государь.",        OpinionOnFulfill = 35, GoldCost = 30, CommonsOpinion = 15 },
        new AmbitionDefinition { Id = AmbitionId.HearMyProphecy,    Title = "Выслушай пророчество",  Demand = "Мне было видение. Про тебя.",       OpinionOnFulfill = 25, OnRefuse = ConsequenceId.SpreadRumor },
        new AmbitionDefinition { Id = AmbitionId.TasteMySoup,       Title = "Попробуй мой суп",      Demand = "Я готовил три дня.",                OpinionOnFulfill = 20, OnRefuse = ConsequenceId.Scandal },
        new AmbitionDefinition { Id = AmbitionId.NameYourDogAfterMe,Title = "Назови собаку в мою честь", Demand = "Пустяк, но мне будет приятно.", OpinionOnFulfill = 30, CourtOpinion = -5 },
    };

    private static ConsequenceDefinition[] DefaultConsequences() => new[]
    {
        C(ConsequenceId.None,               ""),
        C(ConsequenceId.ChallengeToDuel,    "{lord} бросил перчатку. Отказаться нельзя."),
        C(ConsequenceId.Scandal,            "К вечеру об этом знал весь замок."),
        C(ConsequenceId.SpreadRumor,        "{lord} говорил тихо, но говорил со всеми."),
        C(ConsequenceId.DemandGift,         "{lord} ждёт подарка. И ждать не любит."),
        C(ConsequenceId.LeaveCourt,         "{lord} уехал на рассвете. Копья уехали с ним."),
        C(ConsequenceId.PlotMurder,         "{lord} перестал спорить. Это хуже, чем спор."),
        C(ConsequenceId.GetDrunk,           "{lord} упал лицом в блюдо. День окончен."),
        C(ConsequenceId.PublicRepentance,   "{lord} каялся на площади. Крестьянам понравилось."),
        C(ConsequenceId.FallDownStairs,     "Лестница была там же, где вчера. {lord} — нет."),
        C(ConsequenceId.BringExtraTroops,   "{lord} приведёт больше, чем обещал."),
        C(ConsequenceId.TellRival,          "О разговоре узнал тот, кому не следовало."),

        C(ConsequenceId.ChokeAtFeast,       "{lord} подавился на собственном пиру.",        lethalLord: true),
        C(ConsequenceId.FallFromHorse,      "Лошадь решила иначе.",                          lethalLord: true),
        C(ConsequenceId.PoisonedByLover,    "Любовник передумал.",                           lethalLord: true),
        C(ConsequenceId.StruckByLightning,  "Пророчество сбылось буквально.",                lethalLord: true),
        C(ConsequenceId.PlagueInCastle,     "Крысы пришли раньше армии.",                    lethalLord: true, lethalPlayer: true),
        C(ConsequenceId.DuelGoesBadly,      "Ты принял вызов. Зря.",                         lethalPlayer: true),
        C(ConsequenceId.DrownInMoat,        "Ночью, пьяным, во рву собственного замка.",     lethalPlayer: true),
        C(ConsequenceId.AvengedBySon,       "Сын оскорблённого лорда оказался терпелив.",    lethalPlayer: true),

        C(ConsequenceId.LordDiesLaughing,       "{lord} умер со смеху над твоим предложением.", lethalLord: true),
        C(ConsequenceId.BastardClaimsCastle,    "Объявился ещё один наследник."),
        C(ConsequenceId.LoversMeet,             "Два любовника встретились в коридоре."),
        C(ConsequenceId.DogBitesNamesake,       "Собака укусила лорда, в честь которого названа."),
        C(ConsequenceId.ExcommunicatedByBrother,"Брат Одо отлучил тебя лично."),
        C(ConsequenceId.HorseEatsTheTreaty,     "Договор был на столе. Лошадь была рядом."),
        C(ConsequenceId.TurnipToTheFace,        "Крестьянин промахнулся. Или нет."),
        C(ConsequenceId.SoupWasTerrible,        "Ты попробовал. Ты сказал правду."),
        C(ConsequenceId.ProphecyFulfilled,      "Одо был прав, и это хуже всего."),
        C(ConsequenceId.ConfessDrunkenly,       "Ты рассказал всё. Всем."),
    };

    private static VerbReaction R(VerbId verb, int opinion, int chance, ConsequenceId onFail, string note) =>
        new VerbReaction { Verb = verb, Opinion = opinion, Chance = chance, OnFail = onFail, Note = note };

    private static ConsequenceDefinition C(ConsequenceId id, string line,
        bool lethalLord = false, bool lethalPlayer = false) =>
        new ConsequenceDefinition { Id = id, ChronicleLine = line, IsLethalForLord = lethalLord, IsLethalForPlayer = lethalPlayer };
}

// ─────────────────────────────  ОПРЕДЕЛЕНИЯ  ─────────────────────────────

[Serializable]
public class TraitDefinition
{
    public TraitId Id;
    public string Title;
    public string TitleFemale;
    [TextArea(1, 2)] public string Hint;

    [Header("Когда черта на собеседнике")]
    public VerbReaction[] Reactions = new VerbReaction[0];

    [Header("Когда черта на тебе")]
    public VerbReaction[] SelfReactions = new VerbReaction[0];

    [Header("Пассивка")]
    public int GoldPerDay;
    public int FoodPerDay;
    public int CommonsOpinion;      // разовый сдвиг мнения крестьян на старте
    public int OpinionDriftPerDay;  // сам теплеет или остывает

    [Header("Риск каждую ночь")]
    public ConsequenceId DailyRisk;
    [Range(0, 100)] public int DailyRiskChance;

    [Header("Открывает глаголы")]
    public VerbId[] UnlockedVerbs = new VerbId[0];

    public string GetTitle(Gender gender) =>
        gender == Gender.Female && !string.IsNullOrEmpty(TitleFemale) ? TitleFemale : Title;

    public VerbReaction GetReaction(VerbId verb) => Find(Reactions, verb);
    public VerbReaction GetSelfReaction(VerbId verb) => Find(SelfReactions, verb);

    private static VerbReaction Find(VerbReaction[] source, VerbId verb)
    {
        if (source == null) return default;
        for (int i = 0; i < source.Length; i++)
            if (source[i].Verb == verb) return source[i];
        return default;
    }
}

[Serializable]
public struct VerbReaction
{
    public VerbId Verb;
    public int Opinion;            // сдвиг итогового мнения
    public int Chance;             // сдвиг шанса успеха, %
    public ConsequenceId OnFail;   // что случится при провале
    [TextArea(1, 2)] public string Note;  // строка для подсказки в UI
}

[Serializable]
public class VerbDefinition
{
    public VerbId Id;
    public string Title;
    [TextArea(1, 2)] public string Hint;

    [Header("База")]
    public int BaseOpinion;
    [Range(0, 100)] public int BaseChance = 100;

    [Header("Цена")]
    public int GoldCost;
    public int FoodCost;
    public bool OncePerLord;
    public bool RequiresTrait;
    public TraitId RequiredTrait;

    [Header("Кому ещё прилетит")]
    public int RivalOpinion;     // сопернику цели
    public int CommonsOpinion;   // крестьянам
    public int CourtOpinion;     // всем остальным лордам

    [Header("Провал")]
    public ConsequenceId OnFail;
}

[Serializable]
public class AmbitionDefinition
{
    public AmbitionId Id;
    public string Title;
    [TextArea(1, 2)] public string Demand;   // как он это озвучивает

    public int OpinionOnFulfill;
    public int GoldCost;
    public int CommonsOpinion;
    public int CourtOpinion;
    public bool ClosesRomance;
    public ConsequenceId OnRefuse;
}

[Serializable]
public class ConsequenceDefinition
{
    public ConsequenceId Id;
    [TextArea(2, 3)] public string ChronicleLine;   // {lord} и {player} — слоты

    public bool IsLethalForLord;
    public bool IsLethalForPlayer;

    public int LordOpinion;
    public int CourtOpinion;
    public int CommonsOpinion;
    public int Gold;
    public int Food;
    public int Troops;
    public int ActionsLost;
}

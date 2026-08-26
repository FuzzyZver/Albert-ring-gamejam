using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

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
    public string[] Epithets(Gender gender) => gender == Gender.Male ? MaleEpithets : FemaleEpithets;

    // ─────────────────────  СТАРТОВАЯ МАТРИЦА  ─────────────────────
    // Заполняется один раз при создании ассета. Дальше правишь в инспекторе.
    // Если добавил новый глагол в enum — правый клик по ассету,
    // «Добавить недостающие записи», иначе в ассете его не будет.
    // Матрица РАЗРЕЖЕННАЯ: черта реагирует на два-четыре глагола, на остальные молчит.

    private static TraitDefinition[] DefaultTraits() => new[]
    {
        new TraitDefinition {
            Id = TraitId.Proud, Title = "Гордый", TitleFemale = "Гордая",
            Hint = "Не продаётся. Помнит оскорбления.",
            Reactions = new[] {
                R(VerbId.Flatter,       15,  0, ConsequenceId.None,            "любит лесть"),
                R(VerbId.Bribe,        -30,  0, ConsequenceId.None,            "он не продаётся"),
                R(VerbId.Threaten,     -25,  0, ConsequenceId.ChallengeToDuel, "вызовет на поединок"),
                R(VerbId.Insult,       -20,  0, ConsequenceId.ChallengeToDuel, "вызовет на поединок"),
                R(VerbId.AskForCounsel, 15,  0, ConsequenceId.None,            "его наконец спросили"),
                R(VerbId.HuntTogether,   5,  0, ConsequenceId.None,            ""),
            },
            SelfReactions = new[] {
                R(VerbId.Threaten,  5, 0, ConsequenceId.None, "ты умеешь пугать"),
                R(VerbId.Bribe,   -10, 0, ConsequenceId.None, "тебе противно платить"),
            },
            DuelChance = 10,
        },

        new TraitDefinition {
            Id = TraitId.Greedy, Title = "Алчный", TitleFemale = "Алчная",
            Hint = "Всё имеет цену. Особенно он.",
            Reactions = new[] {
                R(VerbId.Bribe,           25, 0, ConsequenceId.None,        "берёт и просит ещё"),
                R(VerbId.Flatter,         -5, 0, ConsequenceId.None,        "словами сыт не будет"),
                R(VerbId.AskForTroops,   -10, 0, ConsequenceId.DemandGift,  "потребует плату"),
                R(VerbId.FulfillAmbition, 10, 0, ConsequenceId.None,        ""),
                R(VerbId.InviteToCastle,  10, 0, ConsequenceId.None,        "поест за твой счёт"),
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
                R(VerbId.Flatter,       -15, 0, ConsequenceId.None,        "видит насквозь"),
                R(VerbId.Bribe,           5, 0, ConsequenceId.TellRival,   "возьмёт и расскажет"),
                R(VerbId.Threaten,       -5, 0, ConsequenceId.SpreadRumor, "пустит слух"),
                R(VerbId.Insult,          5, 0, ConsequenceId.None,        "уважает прямоту"),
                R(VerbId.AskForCounsel,   5, 0, ConsequenceId.SpreadRumor, "расскажет всем, что ты не справляешься"),
                R(VerbId.InviteToCastle,  5, 0, ConsequenceId.TellRival,   "запомнит планировку"),
            },
            SelfReactions = new[] {
                R(VerbId.Flatter,        5, 0, ConsequenceId.None, ""),
                R(VerbId.Insult,         5, 0, ConsequenceId.None, ""),
                R(VerbId.AskForCounsel,  5, 0, ConsequenceId.None, "ты услышишь и то, что не сказали"),
            },
            DuelChance = 5,
        },

        new TraitDefinition {
            Id = TraitId.Pious, Title = "Набожный", TitleFemale = "Набожная",
            Hint = "Судит тебя. Вслух.",
            Reactions = new[] {
                R(VerbId.Seduce,          0, -25, ConsequenceId.None,             "провал = скандал"),
                R(VerbId.DrinkTogether, -10,   0, ConsequenceId.None,             "не пьёт"),
                R(VerbId.Insult,        -15,   0, ConsequenceId.PublicRepentance, "покается прилюдно"),
                R(VerbId.Flatter,         5,   0, ConsequenceId.None,             ""),
                R(VerbId.PrayTogether,   20,   0, ConsequenceId.None,             "наконец-то"),
                R(VerbId.HuntTogether,  -10,   0, ConsequenceId.None,             "убийство ради забавы"),
            },
            SelfReactions = new[] {
                R(VerbId.Seduce,        0, -10, ConsequenceId.None, "тебе стыдно"),
                R(VerbId.PrayTogether, 10,   0, ConsequenceId.None, "ты знаешь слова"),
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
                R(VerbId.HuntTogether,  15, 0, ConsequenceId.None, "любит кровь"),
                R(VerbId.PrayTogether,  -5, 0, ConsequenceId.None, ""),
            },
            SelfReactions = new[] {
                R(VerbId.Threaten,     10, 0, ConsequenceId.None, "тебе верят"),
                R(VerbId.HuntTogether, 10, 0, ConsequenceId.None, ""),
            },
            DuelChance = 15,
            SelfRisk = ConsequenceId.AvengedBySon, SelfRiskChance = 2,
            CommonsOpinion = -10,
        },

        new TraitDefinition {
            Id = TraitId.Craven, Title = "Трусливый", TitleFemale = "Трусливая",
            Hint = "Прогибается. И сбегает.",
            Reactions = new[] {
                R(VerbId.Threaten,       25, 0, ConsequenceId.None,       "сразу сдастся"),
                R(VerbId.AskForTroops,  -20, 0, ConsequenceId.LeaveCourt, "уедет от греха"),
                R(VerbId.Insult,        -10, 0, ConsequenceId.LeaveCourt, "уедет от греха"),
                R(VerbId.HuntTogether,  -15, 0, ConsequenceId.None,       "боится лошадей"),
                R(VerbId.InviteToCastle, 10, 0, ConsequenceId.None,       "за стенами спокойнее"),
            },
            SelfReactions = new[] {
                R(VerbId.Threaten,     -15, 0, ConsequenceId.None, "тебе не верят"),
                R(VerbId.AskForTroops,  -5, 0, ConsequenceId.None, ""),
                R(VerbId.HuntTogether, -10, 0, ConsequenceId.None, "ты и сам не любишь лошадей"),
            },
            DuelChance = -25,
        },

        new TraitDefinition {
            Id = TraitId.Lustful, Title = "Похотливый", TitleFemale = "Похотливая",
            Hint = "Согласен почти на всё.",
            Reactions = new[] {
                R(VerbId.Seduce,          0, 30, ConsequenceId.None, "почти согласен"),
                R(VerbId.DrinkTogether,  10,  0, ConsequenceId.None, ""),
                R(VerbId.Flatter,        10,  0, ConsequenceId.None, ""),
                R(VerbId.InviteToCastle, 10,  0, ConsequenceId.None, "у тебя в замке есть на кого посмотреть"),
            },
            SelfReactions = new[] {
                R(VerbId.Seduce, 0, 15, ConsequenceId.None, "ты знаешь, что делаешь"),
            },
        },

        new TraitDefinition {
            Id = TraitId.Drunkard, Title = "Пьяница", TitleFemale = "Пьяница",
            Hint = "Лучший друг за столом. Худший — наутро.",
            Reactions = new[] {
                R(VerbId.DrinkTogether,  25, 0, ConsequenceId.GetDrunk, "напьётся"),
                R(VerbId.Bribe,          10, 0, ConsequenceId.None,     ""),
                R(VerbId.Threaten,       -5, 0, ConsequenceId.None,     ""),
                R(VerbId.PrayTogether,  -10, 0, ConsequenceId.None,     "не в этом состоянии"),
                R(VerbId.InviteToCastle, 15, 0, ConsequenceId.None,     "у тебя погреб"),
            },
            SelfReactions = new[] {
                R(VerbId.DrinkTogether,  15, 0, ConsequenceId.ConfessDrunkenly, "шанс проболтаться", 35),
                R(VerbId.InviteToCastle, 10, 0, ConsequenceId.None,             ""),
            },
            DuelChance = -10,
            DailyRisk = ConsequenceId.FallDownStairs, DailyRiskChance = 4,
            SelfRisk = ConsequenceId.DrownInMoat, SelfRiskChance = 3,
        },

        new TraitDefinition {
            Id = TraitId.Obsessed, Title = "Одержимый", TitleFemale = "Одержимая",
            Hint = "У него одна мысль. Ты в ней мешаешь.",
            Reactions = new[] {
                R(VerbId.FulfillAmbition, 30, 0, ConsequenceId.None,       "только это и важно"),
                R(VerbId.Flatter,        -10, 0, ConsequenceId.None,       "не слышит"),
                R(VerbId.Bribe,          -10, 0, ConsequenceId.None,       "не слышит"),
                R(VerbId.Insult,         -25, 0, ConsequenceId.PlotMurder, "начнёт готовить убийство"),
                R(VerbId.AskForCounsel,    5, 0, ConsequenceId.None,       "расскажет только о своём"),
            },
            SelfReactions = new[] {
                R(VerbId.FulfillAmbition, 10, 0, ConsequenceId.None, ""),
            },
            DailyRisk = ConsequenceId.ProphecyFulfilled, DailyRiskChance = 3,
            SelfRisk = ConsequenceId.StruckByLightning, SelfRiskChance = 2,
        },

        new TraitDefinition {
            Id = TraitId.Honest, Title = "Честный", TitleFemale = "Честная",
            Hint = "Не врёт. Тебе тоже не даст.",
            Reactions = new[] {
                R(VerbId.Bribe,         -25, 0, ConsequenceId.TellRival, "донесёт о взятке"),
                R(VerbId.Flatter,       -10, 0, ConsequenceId.None,      "не любит лести"),
                R(VerbId.Threaten,        5, 0, ConsequenceId.None,      "ценит прямоту"),
                R(VerbId.AskForTroops,   15, 0, ConsequenceId.None,      "слово держит"),
                R(VerbId.AskForCounsel,  10, 0, ConsequenceId.None,      "скажет правду, какой бы она ни была"),
            },
            SelfReactions = new[] {
                R(VerbId.Bribe,         -15, 0, ConsequenceId.None, "у тебя дрожат руки"),
                R(VerbId.Flatter,       -10, 0, ConsequenceId.None, "звучит фальшиво"),
                R(VerbId.AskForTroops,   10, 0, ConsequenceId.None, "тебе верят"),
                R(VerbId.AskForCounsel,   5, 0, ConsequenceId.None, ""),
            },
            CommonsOpinion = 10,
        },
    };

    private static VerbDefinition[] DefaultVerbs() => new[]
    {
        new VerbDefinition {
            Id = VerbId.Flatter, Title = "Польстить", Hint = "Бесплатно, но приедается",
            BaseOpinion = 10, BaseChance = 100, RepeatPenalty = 5,
        },
        new VerbDefinition {
            Id = VerbId.Bribe, Title = "Подкупить", Hint = "Деньги решают. Не всё",
            BaseOpinion = 15, BaseChance = 100, GoldCost = 15, RepeatPenalty = 4,
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
            BaseOpinion = 30, BaseChance = 45, OnFail = ConsequenceId.Scandal, OpinionOnFail = -15,
            RequiresTrait = true, RequiredTrait = TraitId.Lustful,
        },
        new VerbDefinition {
            Id = VerbId.Insult, Title = "Послать на три буквы", Hint = "Радует соперника и крестьян",
            BaseOpinion = -45, BaseChance = 100, RivalOpinion = 30, CommonsOpinion = 10,
        },
        new VerbDefinition {
            Id = VerbId.FulfillAmbition, Title = "Исполнить желание", Hint = "Один раз на лорда",
            BaseOpinion = 0, BaseChance = 100, OncePerLord = true, RivalOpinion = -20,
        },
        new VerbDefinition {
            Id = VerbId.AskForTroops, Title = "Просить войск", Hint = "Унизительно. И он ещё может отказать",
            BaseOpinion = -10, BaseChance = 100, CooldownDays = 1, OpinionOnFail = -10,
        },

        new VerbDefinition {
            Id = VerbId.PrayTogether, Title = "Помолиться вместе", Hint = "Крестьяне это видят",
            BaseOpinion = 8, BaseChance = 100, CommonsOpinion = 5,
        },
        new VerbDefinition {
            Id = VerbId.HuntTogether, Title = "Позвать на охоту", Hint = "Загон завтра вечером. Лошади бывают разные",
            BaseOpinion = 12, BaseChance = 90, FoodCost = 8, OnFail = ConsequenceId.FallFromHorse,
        },
        new VerbDefinition {
            Id = VerbId.InviteToCastle, Title = "Позвать в замок", Hint = "Гость увидит всё, что у тебя есть",
            BaseOpinion = 20, BaseChance = 100, GoldCost = 10, FoodCost = 10, CourtOpinion = -3,
        },
        new VerbDefinition {
            Id = VerbId.AskForCounsel, Title = "Спросить совета", Hint = "Бесплатно. Почти",
            BaseOpinion = 6, BaseChance = 100,
        },
    };

    private static AmbitionDefinition[] DefaultAmbitions() => new[]
    {
        new AmbitionDefinition { Id = AmbitionId.MarryMyDaughter,    Title = "Женись на моей дочери",     PlayerAction = "Жениться на дочери",        Demand = "У меня дочь на выданье, государь.", OpinionOnFulfill = 50, ClosesRomance = true, CourtOpinion = -5 },
        new AmbitionDefinition { Id = AmbitionId.GiveMeTheMill,      Title = "Отдай мельницу",            PlayerAction = "Отдать мельницу",           Demand = "Мельница у брода должна быть моей.", OpinionOnFulfill = 40, CommonsOpinion = -15 },
        new AmbitionDefinition { Id = AmbitionId.GrantMeATitle,      Title = "Дай титул",                 PlayerAction = "Дать титул",                Demand = "Я достоин большего, чем есть.",      OpinionOnFulfill = 45, CourtOpinion = -10 },
        new AmbitionDefinition { Id = AmbitionId.KillMyRival,        Title = "Убей моего врага",          PlayerAction = "Убить его врага",           Demand = "Ты знаешь, о ком я.",                OpinionOnFulfill = 60, CourtOpinion = -20 },
        new AmbitionDefinition { Id = AmbitionId.BuildTheChapel,     Title = "Построй часовню",           PlayerAction = "Построить часовню",         Demand = "Господь смотрит, государь.",         OpinionOnFulfill = 35, GoldCost = 30, CommonsOpinion = 15 },
        new AmbitionDefinition { Id = AmbitionId.HearMyProphecy,     Title = "Выслушай пророчество",      PlayerAction = "Выслушать пророчество",     Demand = "Мне было видение. Про тебя.",        OpinionOnFulfill = 25 },
        new AmbitionDefinition { Id = AmbitionId.TasteMySoup,        Title = "Попробуй мой суп",          PlayerAction = "Попробовать суп",           Demand = "Я готовил три дня.",                 OpinionOnFulfill = 20 },
        new AmbitionDefinition { Id = AmbitionId.NameYourDogAfterMe, Title = "Назови собаку в мою честь", PlayerAction = "Назвать собаку в его честь", Demand = "Пустяк, но мне будет приятно.",     OpinionOnFulfill = 30, CourtOpinion = -5 },
    };

    private static ConsequenceDefinition[] DefaultConsequences() => new[]
    {
        C(ConsequenceId.None,             "",                    ""),
        C(ConsequenceId.ChallengeToDuel,  "поединок",            "{lord} бросил перчатку. Отказаться нельзя."),
        C(ConsequenceId.Scandal,          "скандал",             "К вечеру об этом знал весь замок.",           court: -10, commons: -5),
        C(ConsequenceId.SpreadRumor,      "слух",                "{lord} говорил тихо, но говорил со всеми.",   court: -8),
        C(ConsequenceId.DemandGift,       "требует подарок",     "{lord} ждёт подарка. И ждать не любит.",      lord: -5),
        C(ConsequenceId.LeaveCourt,       "уедет",               "{lord} уехал на рассвете. Копья уехали с ним."),
        C(ConsequenceId.PlotMurder,       "заговор",             "{lord} перестал спорить. Это хуже, чем спор."),
        C(ConsequenceId.GetDrunk,         "напьётся",            "{lord} упал лицом в блюдо. День окончен.",    actionsLost: 1),
        C(ConsequenceId.PublicRepentance, "покается прилюдно",   "{lord} каялся на площади. Крестьянам понравилось.", commons: 10),
        C(ConsequenceId.FallDownStairs,   "падение с лестницы",  "Лестница была там же, где вчера. {lord} — нет."),
        C(ConsequenceId.BringExtraTroops, "приведёт больше",     "{lord} приведёт больше, чем обещал.",         troops: 8),
        C(ConsequenceId.TellRival,        "донесёт сопернику",   "О разговоре узнал тот, кому не следовало."),

        C(ConsequenceId.ChokeAtFeast,      "подавился",       "{lord} подавился на собственном пиру.",     lethalLord: true),
        C(ConsequenceId.FallFromHorse,     "лошадь решила иначе", "Лошадь решила иначе.",                  lethalLord: true),
        C(ConsequenceId.PoisonedByLover,   "яд",              "Любовник передумал.",                       lethalLord: true),
        C(ConsequenceId.StruckByLightning, "молния",          "Пророчество сбылось буквально.",            lethalLord: true, lethalPlayer: true),
        C(ConsequenceId.PlagueInCastle,    "чума",            "Крысы пришли раньше армии.",                lethalLord: true, lethalPlayer: true),
        C(ConsequenceId.DuelGoesBadly,     "поединок проигран", "Ты принял вызов. Зря.",                   lethalPlayer: true),
        C(ConsequenceId.DrownInMoat,       "ров",             "Ночью, пьяным, во рву собственного замка.", lethalPlayer: true),
        C(ConsequenceId.AvengedBySon,      "месть сына",      "Сын оскорблённого лорда оказался терпелив.", lethalPlayer: true),

        C(ConsequenceId.LordDiesLaughing,        "умер со смеху", "{lord} умер со смеху над твоим предложением.", lethalLord: true),
        C(ConsequenceId.BastardClaimsCastle,     "ещё наследник", "Объявился ещё один наследник."),
        C(ConsequenceId.LoversMeet,              "встреча в коридоре", "Два любовника встретились в коридоре.", court: -10),
        C(ConsequenceId.DogBitesNamesake,        "собака укусила", "Собака укусила лорда, в честь которого названа.", lord: -15),
        C(ConsequenceId.ExcommunicatedByBrother, "отлучение",   "Брат Одо отлучил тебя лично.",                commons: -20),
        C(ConsequenceId.HorseEatsTheTreaty,      "лошадь съела договор", "Договор был на столе. Лошадь была рядом."),
        C(ConsequenceId.TurnipToTheFace,         "репа в лицо", "Крестьянин промахнулся. Или нет.",            commons: -5),
        C(ConsequenceId.SoupWasTerrible,         "суп был ужасен", "Ты попробовал. Ты сказал правду.",         lord: -30),
        C(ConsequenceId.ProphecyFulfilled,       "пророчество сбылось", "Одо был прав, и это хуже всего."),
        C(ConsequenceId.ConfessDrunkenly,        "ты рассказал всё", "Ты рассказал всё. Всем.",                court: -12),
    };

    private static VerbReaction R(VerbId verb, int opinion, int chance, ConsequenceId consequence, string note,
        int consequenceChance = 100) =>
        new VerbReaction
        {
            Verb = verb,
            Opinion = opinion,
            Chance = chance,
            Consequence = consequence,
            ConsequenceChance = consequenceChance,
            Note = note,
        };

    private static ConsequenceDefinition C(ConsequenceId id, string title, string line,
        bool lethalLord = false, bool lethalPlayer = false,
        int lord = 0, int court = 0, int commons = 0,
        int gold = 0, int food = 0, int troops = 0, int actionsLost = 0) =>
        new ConsequenceDefinition
        {
            Id = id,
            Title = title,
            ChronicleLine = line,
            IsLethalForLord = lethalLord,
            IsLethalForPlayer = lethalPlayer,
            LordOpinion = lord,
            CourtOpinion = court,
            CommonsOpinion = commons,
            Gold = gold,
            Food = food,
            Troops = troops,
            ActionsLost = actionsLost,
        };

    // ─────────────────────  ИМЕНА И ФОРМАТИРОВАНИЕ  ─────────────────────
    // Живёт в конфиге, а не в статик-хелпере: именно конфиг владеет названиями,
    // и системам не нужно знать ни про род, ни про падежи.

    public string TraitTitle(TraitId id, Gender gender)
    {
        var trait = GetTrait(id);
        return trait != null ? trait.GetTitle(gender) : id.ToString();
    }

    public string TraitLine(TraitId a, TraitId b, Gender gender) =>
        TraitTitle(a, gender) + ", " + TraitTitle(b, gender);

    /// <summary>Две строки-подсказки под кандидатом: во что ты ввязываешься.</summary>
    public string TraitHints(TraitId a, TraitId b)
    {
        var text = new StringBuilder();
        Append(text, GetTrait(a));
        Append(text, GetTrait(b));
        return text.ToString();
    }

    public string AmbitionTitle(AmbitionId id)
    {
        var ambition = GetAmbition(id);
        return ambition != null ? ambition.Title : id.ToString();
    }

    private static void Append(StringBuilder text, TraitDefinition trait)
    {
        if (trait == null || string.IsNullOrEmpty(trait.Hint)) return;
        if (text.Length > 0) text.AppendLine();
        text.Append(trait.GetTitle(Gender.Male)).Append(" — ").Append(trait.Hint);
    }

    // ─────────────────────  ДОБОР НОВЫХ ЗАПИСЕЙ  ─────────────────────

    /// <summary>Полная перезапись значениями из кода. Нужна, когда данные в ассете
    /// разошлись со схемой: правки в инспекторе при этом теряются.</summary>
    [ContextMenu("Пересобрать матрицу с нуля")]
    private void ResetToDefaults()
    {
        Traits = DefaultTraits();
        Verbs = DefaultVerbs();
        Ambitions = DefaultAmbitions();
        Consequences = DefaultConsequences();

        Debug.LogWarning($"{name}: матрица перезаписана значениями из кода.", this);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Добавить недостающие записи")]
    private void FillMissing()
    {
        int added = 0;
        Traits = Merge(Traits, DefaultTraits(), t => (int)t.Id, ref added);
        Verbs = Merge(Verbs, DefaultVerbs(), v => (int)v.Id, ref added);
        Ambitions = Merge(Ambitions, DefaultAmbitions(), a => (int)a.Id, ref added);
        Consequences = Merge(Consequences, DefaultConsequences(), c => (int)c.Id, ref added);

        Debug.Log(added > 0 ? $"{name}: дописано записей — {added}" : $"{name}: всё на месте", this);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private static T[] Merge<T>(T[] current, T[] defaults, Func<T, int> id, ref int added)
    {
        var list = new List<T>(current ?? new T[0]);

        for (int d = 0; d < defaults.Length; d++)
        {
            bool exists = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (id(list[i]) != id(defaults[d])) continue;
                exists = true;
                break;
            }

            if (exists) continue;
            list.Add(defaults[d]);
            added++;
        }

        return list.ToArray();
    }
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

    [Header("Риск каждую ночь, если черта на лорде")]
    public ConsequenceId DailyRisk;
    [Range(0, 100)] public int DailyRiskChance;

    [Header("Риск каждую ночь, если черта на тебе")]
    public ConsequenceId SelfRisk;
    [Range(0, 100)] public int SelfRiskChance;

    [Header("Поединок")]
    public int DuelChance;   // сдвиг шанса победить: на тебе — в плюс, на противнике — в минус

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
    public int Opinion;   // сдвиг итогового мнения
    public int Chance;    // сдвиг шанса успеха, %

    /// <summary>Что случится, когда к нему применят этот глагол.
    /// Срабатывает НЕЗАВИСИМО от того, удался глагол или нет: Гордый вызывает
    /// на поединок именно потому, что ты успешно ему пригрозил.
    /// Провал самого броска описывается отдельно — в VerbDefinition.OnFail.</summary>
    [FormerlySerializedAs("OnFail")]
    public ConsequenceId Consequence;
    [Range(0, 100)] public int ConsequenceChance;   // 0 = всегда

    [TextArea(1, 2)] public string Note;  // строка для разбора в карточке

    public bool IsEmpty => Opinion == 0 && Chance == 0 && Consequence == ConsequenceId.None;
    public int RealChance => ConsequenceChance <= 0 ? 100 : ConsequenceChance;
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
    public int CooldownDays;     // 0 = хоть каждый день
    public int RepeatPenalty;    // сколько мнения теряет каждое повторение: приедается
    public bool RequiresTrait;
    public TraitId RequiredTrait;

    [Header("Кому ещё прилетит")]
    public int RivalOpinion;     // сопернику цели
    public int CommonsOpinion;   // крестьянам
    public int CourtOpinion;     // всем остальным лордам

    [Header("Провал")]
    public ConsequenceId OnFail;
    public int OpinionOnFail;    // отказ тоже стоит лица
}

[Serializable]
public class AmbitionDefinition
{
    public AmbitionId Id;
    public string Title;         // как он это просит: «Женись на моей дочери»
    public string PlayerAction;  // как это делаешь ты: «Жениться на дочери»
    [TextArea(1, 2)] public string Demand;

    public int OpinionOnFulfill;
    public int GoldCost;
    public int CommonsOpinion;
    public int CourtOpinion;
    public bool ClosesRomance;
}

[Serializable]
public class ConsequenceDefinition
{
    public ConsequenceId Id;
    public string Title;                            // короткое имя для разбора: «скандал»
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
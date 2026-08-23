// ═══ ATTRIBUTE ═══ свойства сущности, живут весь забег

// ─────────── персонаж ───────────

/// <summary>Стабильный id, -1 у игрока. Именно его храним во всех долгих ссылках,
/// а не EcsEntity: обращение к мёртвой сущности падает, мёртвый id — просто не найдётся.</summary>
public struct LordIdAttribute
{
    public int Value;
}

public struct PersonAttribute
{
    public string Title;
    public string GivenName;
    public string Epithet;
    public Gender Gender;

    public string FullName => string.IsNullOrEmpty(Epithet)
        ? Title + " " + GivenName
        : Title + " " + GivenName + " " + Epithet;
}

public struct TraitsAttribute
{
    public TraitId A;
    public TraitId B;

    public bool Has(TraitId id) => A == id || B == id;
}

public struct OpinionAttribute { public int Value; }

public struct TroopsAttribute { public int Value; }

public struct AmbitionAttribute { public AmbitionId Id; }

public struct RivalAttribute { public int LordId; }

/// <summary>Одно применение глагола к лорду. Дата нужна и для «один раз за забег»,
/// и для кулдауна, и для того, чтобы лесть приедалась.</summary>
public struct VerbUse
{
    public VerbId Verb;
    public int Day;
}

/// <summary>Вся история разговоров с этим лордом.</summary>
public struct VerbHistoryAttribute { public System.Collections.Generic.List<VerbUse> Value; }

// ─────────── забег ───────────

public struct CalendarAttribute
{
    public int Day;
    public DayPhase Phase;
    public int ActionsLeft;
}

public struct TreasuryAttribute
{
    public int Gold;
    public int Food;
    public int Garrison;
}

public struct TaxAttribute
{
    public int Peasants;   // ползунок 0..3
    public int Lords;      // ползунок 0..3
}

public struct CommonsAttribute { public int Opinion; }

/// <summary>Крестьяне помнят. Каждая ночь повышенной подати добавляет злости,
/// спокойная — гасит. Злость вычитается из мнения СЛЕДУЮЩЕЙ ночью,
/// поэтому подымать налог на день выгодно, а на неделю — самоубийство.</summary>
public struct CommonsMemoryAttribute { public int Grudge; }

public struct RngAttribute
{
    public int Seed;
    public System.Random Value;
}

/// <summary>Сгенерированный забег целиком. Нужен, пока игрок не выбрал себя:
/// кандидаты должны где-то дожить до нажатия кнопки.</summary>
public struct CourtAttribute { public CourtData Value; }

/// <summary>На что игрок смотрит прямо сейчас. Отдельно от фазы:
/// фаза — состояние игры, экран — состояние взгляда.</summary>
public struct ScreenAttribute { public ScreenId Current; }

/// <summary>Кого игрок сейчас разглядывает. Живёт в компоненте, а не в поле системы:
/// на выделение смотрят и карточка, и панель глаголов, и сбрасываться оно должно
/// вместе с забегом — в RunSetupSystem это происходит само.</summary>
public struct SelectionAttribute
{
    public const int Nobody = int.MinValue;
    public const int Player = -1;

    public int LordId;

    public bool HasTarget => LordId != Nobody;
    public bool IsPlayer => LordId == Player;
}

/// <summary>Строки, посчитанные для выделенного лорда. Единственный источник правды
/// о том, что сейчас можно сделать: применение читает отсюда, а не пересчитывает.</summary>
public struct VerbOffersAttribute
{
    public System.Collections.Generic.List<VerbOutcome> Value;
}

/// <summary>Сколько ночей подряд амбары пусты. Голод убивает не сразу — успеваешь заметить.</summary>
public struct StarvingAttribute { public int Nights; }

/// <summary>Кто вызвал тебя на поединок. -1 — никто.</summary>
public struct DuelAttribute { public int LordId; public int Chance; }

/// <summary>Что показывает вечерний экран прямо сейчас. Заполняют системы событий,
/// рисует EveningSystem. Waiting значит «ждём выбора игрока» — пока он стоит,
/// фаза не двигается.</summary>
public struct EveningAttribute
{
    public EveningKind Kind;
    public int LordId;
    public string Title;
    public string Body;
    public string Choice;
    public bool Waiting;
}

/// <summary>Чем всё кончилось. Заполняется один раз, читается эпилогом.</summary>
public struct RunEndAttribute
{
    public bool Victory;
    public DeathCause Cause;
    public int KillerLordId;
    public string Detail;
    public int Day;
    public int Defence;
    public int SiegeStrength;
}

/// <summary>Что заготовлено в замке на завтрашний вечер.</summary>
public struct PlanAttribute
{
    public bool HasPlan;
    public CastleSlotId Slot;
    public int PlannedOnDay;
}

/// <summary>Предварительный ночной счёт. Считается заново на каждое движение ползунка
/// и применяется только когда игрок ляжет спать — в этом весь смысл ночного окна.</summary>
public struct NightReportAttribute
{
    public int GoldIncome;
    public int GoldUpkeep;
    public int FoodIncome;
    public int FoodUpkeep;
    public int LordOpinionDelta;
    public int CommonsOpinionDelta;
    public int MemoryPenalty;      // сколько из CommonsOpinionDelta — это старые обиды
    public bool Starving;

    public int GoldNet => GoldIncome - GoldUpkeep;
    public int FoodNet => FoodIncome - FoodUpkeep;
}
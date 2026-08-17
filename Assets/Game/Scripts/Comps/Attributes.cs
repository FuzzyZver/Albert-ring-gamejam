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

/// <summary>Какие глаголы уже потрачены на этом лорде (OncePerLord).</summary>
public struct SpentVerbsAttribute { public System.Collections.Generic.List<VerbId> Value; }

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

public struct RngAttribute
{
    public int Seed;
    public System.Random Value;
}

/// <summary>Сгенерированный забег целиком. Нужен, пока игрок не выбрал себя:
/// кандидаты должны где-то дожить до нажатия кнопки.</summary>
public struct CourtAttribute { public CourtData Value; }
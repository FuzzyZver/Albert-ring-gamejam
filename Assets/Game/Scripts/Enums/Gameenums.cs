public enum DayPhase
{
    Morning,   // просители у трона
    Day,       // действия игрока
    Evening,   // событие по триггерам
    Night      // подсчёт и окно на исправление
}

/// <summary>Экран — это не фаза. Фаза говорит, что происходит в игре,
/// экран — на что ты сейчас смотришь. Карту можно листать и утром, и днём.</summary>
public enum ScreenId
{
    None,
    Map,
    Court,
    Castle,
    Evening,
    Night,
    Epilogue
}

/// <summary>Постройки в замке. Новые дописывать только в конец.</summary>
public enum BuildingId
{
    Market,    // рыночная площадь
    Walls,     // стены и башни
    Temple,    // храм
    Brothel    // публичные дома
}

/// <summary>Что можно устроить с булавки действий.</summary>
public enum CastleActionId
{
    Feast,           // пир
    TempleService,   // служба в храме
    HireMercenaries  // вербовка наёмников
}

/// <summary>Кто пришёл к трону утром. Новые дописывать только в конец.</summary>
public enum PetitionId
{
    LandlessKnight,     // рыцарь без надела
    BanditsInTheWood,   // крестьянин: разбойники
    BurnedMill,         // мельник
    LowerTheToll,       // городской голова
    AbsentFromMass,     // настоятель про лорда
    SoldiersWidow,      // вдова
    MercenaryCaptain,   // капитан наёмников
    Informer            // доносчик
}

/// <summary>Вечерние события из конфига. Новые дописывать только в конец.</summary>
public enum EveningEventId
{
    QuietEvening,
    Feast,
    DrunkenNight,
    ProphecyAtTable,
    LoversInTheHall,
    PeasantsAtTheGate,
    RumorSpreads
}

/// <summary>Что происходит вечером. Пока два вида, но экран рассчитан на рост:
/// пиры, молебны, доносы и выборы игрока лягут сюда же.</summary>
public enum EveningKind
{
    None,
    Story,   // событие из EventsConfig
    Duel
}

public enum TaxKind
{
    Peasants,   // подать пищей
    Lords       // пошлина золотом
}

public enum DeathCause
{
    None,
    Riot,           // бунт крестьян
    Famine,         // голод
    Assassination,  // нож в спину
    Overthrow,      // свержение лордами
    Duel,           // поединок
    Accident,       // собственная черта убила
    Siege           // не выстоял осаду
}
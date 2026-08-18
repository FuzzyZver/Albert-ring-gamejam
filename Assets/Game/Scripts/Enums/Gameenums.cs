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
    Night
}

/// <summary>Булавки внутри замка. Стройка — постоянные улучшения,
/// сборы — то, что выстрелит завтра вечером (пир, молебен, суд).</summary>
public enum CastleSlotId
{
    Construction,
    Gathering
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
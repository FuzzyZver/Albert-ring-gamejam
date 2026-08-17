public enum DayPhase
{
    Morning,   // просители у трона
    Day,       // действия игрока
    Evening,   // событие по триггерам
    Night      // подсчёт и окно на исправление
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
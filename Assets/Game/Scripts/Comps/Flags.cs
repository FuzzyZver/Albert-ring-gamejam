// ═══ FLAG ═══ пустые метки: кто это и в каком он состоянии

// кто это
public struct RunFlag { }        // единственная сущность забега: календарь, казна, налоги, rng
public struct PlayerFlag { }
public struct LordFlag { }

// состояние лорда
public struct AtCourtFlag { }              // здесь и доступен для глаголов
public struct PetitionerFlag { }           // сегодня пришёл с просьбой
public struct LoverFlag { }                // роман состоялся
public struct AmbitionFulfilledFlag { }
public struct VengefulFlag { }             // получил повод и будет мстить
public struct PlottingFlag { }             // готовит убийство
public struct DrunkFlag { }
public struct ScandalFlag { }
public struct LeftCourtFlag { }
public struct DeadFlag { }

/// <summary>Выделение или его цифры изменились — пересчитать и перерисовать.
/// Помечен как OneFrame: снимается в конце кадра, когда все вьюхи уже прочитали.</summary>
public struct SelectionChangedFlag { }

// состояние забега
public struct StarvingFlag { }             // пища на нуле
public struct SiegeReadyFlag { }
public struct RunOverFlag { }
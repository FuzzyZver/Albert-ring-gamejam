// ═══ FLAG ═══ пустые метки: кто это и в каком он состоянии

// кто это
public struct RunFlag { }        // единственная сущность забега: календарь, казна, налоги, rng
public struct PlayerFlag { }
public struct LordFlag { }

// состояние лорда
public struct LoverFlag { }                // роман состоялся
public struct AmbitionFulfilledFlag { }
public struct VengefulFlag { }             // получил повод и будет мстить
public struct PlottingFlag { }             // готовит убийство
public struct ScandalFlag { }
public struct LeftCourtFlag { }
public struct DeadFlag { }

/// <summary>Выделение или его цифры изменились — пересчитать и перерисовать.
/// Помечен как OneFrame: снимается в конце кадра, когда все вьюхи уже прочитали.</summary>
public struct SelectionChangedFlag { }

/// <summary>Фазу нельзя двигать, пока игрок не разберётся с тем, что перед ним.</summary>
public struct PhaseLockFlag { }

/// <summary>Игрок принял вызов. Намеренно НЕ событие: EveningSystem рисует экран
/// и потому стоит в конвейере позже DuelSystem. Одно-кадровое событие, созданное
/// после потребителя, стирается в конце того же кадра и до него не доживает.
/// Обычный флаг переживает кадр — DuelSystem снимает его сам.</summary>
public struct DuelAcceptedFlag { }

// состояние забега
public struct StarvingFlag { }             // пища на нуле
public struct RunOverFlag { }
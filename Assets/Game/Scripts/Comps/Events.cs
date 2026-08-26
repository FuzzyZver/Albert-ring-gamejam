using Leopotam.Ecs;

// ═══ EVENT ═══ одно-кадровые сообщения. Здесь EcsEntity держать можно:
// событие создаётся и съедается в том же кадре, цель гарантированно жива.
// Всё, что переживает кадр (соперник, память, цель заговора) — хранит int LordId.

// ─────────── забег ───────────

public struct NewRunEvent { public int Seed; }        // снести всё и сгенерировать заново
public struct CourtReadyEvent { }                     // двор есть, игрока ещё нет
public struct SelectCandidateEvent { public int Index; }
public struct RunReadyEvent { }                       // игрок выбран, можно показывать карту

// ─────────── цикл дня ───────────

public struct AdvancePhaseEvent { }                        // просьба сдвинуть фазу
/// <summary>Фаза, которая только что закончилась. День едет в самом событии:
/// к моменту, когда его прочитают, календарь уже перевернулся на следующий.</summary>
public struct PhaseEndedEvent { public DayPhase Phase; public int Day; }
public struct PhaseChangedEvent { public DayPhase Phase; } // фаза, которая началась

// ─────────── экраны и замок ───────────

public struct ChangeScreenEvent { public ScreenId Target; }
public struct BuildingPinClickedEvent { public BuildingId Id; }
public struct BuildRequestEvent { public BuildingId Id; public int Level; }
public struct CastleActionsPinClickedEvent { }
public struct CastleActionRequestEvent { public CastleActionId Id; }
public struct CloseCastleCardEvent { }
public struct SetTaxEvent { public TaxKind Kind; public int Value; }

// ─────────── взаимодействие ───────────

public struct PinClickedEvent { public EcsEntity Target; }
public struct CloseCardEvent { }

public struct VerbEvent
{
    public int TargetLordId;
    public VerbId Verb;
}

public struct OpinionChangeEvent
{
    public EcsEntity Target;
    public int Delta;
    public string Reason;      // строка для всплывашки: «Гордый: не продаётся»
}

public struct CommonsOpinionChangeEvent
{
    public int Delta;
    public string Reason;
}

public struct CourtOpinionChangeEvent   // всем лордам, кроме исключённого
{
    public int Delta;
    public int ExceptLordId;
    public string Reason;
}

public struct ConsequenceEvent
{
    public EcsEntity Source;
    public ConsequenceId Id;
}

public struct ChronicleEvent { public string Line; }

public struct DeathEvent
{
    public DeathCause Cause;
    public int KillerLordId;   // -1, если виновных нет
    public string Detail;      // строка последствия, если смерть пришла через него
}

public struct VictoryEvent { public int Defence; }
public struct SiegeStartedEvent { }
public struct BattlePinClickedEvent { public int Point; }
public struct CloseBattleCardEvent { }

public struct EveningChoiceEvent { public int Index; }
public struct PetitionChoiceEvent { public int Index; }
public struct CallNextPetitionerEvent { }

/// <summary>Положить событие в вечернюю очередь. Точка расширения: пока никто не пишет,
/// потому что пир и поединок вечер собирает сам из PlanAttribute и DuelAttribute.
/// Понадобится, когда событие захочет назначить другое событие.</summary>
public struct QueueEveningEvent
{
    public EveningKind Kind;
    public EveningEventId Id;
    public int LordId;
}

/// <summary>Применить последствия выбранного варианта. И проситель, и вечернее
/// событие шлют одно и то же — арифметика живёт в ChoiceEffectSystem.</summary>
public struct ApplyChoiceEvent
{
    public ChoiceDefinition Choice;
    public int LordId;
    public string Result;
}
using Leopotam.Ecs;

// ═══ EVENT ═══ одно-кадровые сообщения. Здесь EcsEntity держать можно:
// событие создаётся и съедается в том же кадре, цель гарантированно жива.
// Всё, что переживает кадр (соперник, память, цель заговора) — хранит int LordId.

// ─────────── забег ───────────

public struct NewRunEvent { public int Seed; }        // снести всё и сгенерировать заново
public struct CourtReadyEvent { }                     // двор есть, игрока ещё нет
public struct SelectCandidateEvent { public int Index; }
public struct RunReadyEvent { }                       // игрок выбран, можно показывать карту
public struct RunStartEvent { public int Seed; }

// ─────────── цикл дня ───────────

public struct AdvancePhaseEvent { }                        // просьба сдвинуть фазу
public struct PhaseEndedEvent { public DayPhase Phase; }   // фаза, которая только что закончилась
public struct PhaseChangedEvent { public DayPhase Phase; } // фаза, которая началась
public struct DayStartedEvent { public int Day; }
public struct SpendActionEvent { public int Amount; }

// ─────────── экраны и замок ───────────

public struct ChangeScreenEvent { public ScreenId Target; }
public struct CastleSlotClickedEvent { public CastleSlotId Slot; }
public struct SetTaxEvent { public TaxKind Kind; public int Value; }

// ─────────── взаимодействие ───────────

public struct PinClickedEvent { public EcsEntity Target; }
public struct CloseCardEvent { }

public struct VerbEvent
{
    public EcsEntity Source;   // обычно игрок
    public EcsEntity Target;
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

public struct AmbitionDemandEvent { public EcsEntity Lord; }

public struct NextPhaseEvent { }

public struct DayEndEvent { }

public struct ChronicleEvent { public string Line; }

public struct DeathEvent
{
    public DeathCause Cause;
    public int KillerLordId;
}
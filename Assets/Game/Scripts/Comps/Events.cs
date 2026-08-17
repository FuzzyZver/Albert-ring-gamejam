using Leopotam.Ecs;

// ═══ EVENT ═══ одно-кадровые сообщения. Здесь EcsEntity держать можно:
// событие создаётся и съедается в том же кадре, цель гарантированно жива.
// Всё, что переживает кадр (соперник, память, цель заговора) — хранит int LordId.

public struct RunStartEvent { public int Seed; }

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

public struct RestartRunEvent { public int Seed; }

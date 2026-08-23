/// <summary>Последствие, которое повесила черта. Срабатывает при применении глагола
/// независимо от того, удался он или нет — в отличие от VerbOutcome.OnFail.</summary>
public struct VerbConsequence
{
    public ConsequenceId Id;
    public int Chance;
}

/// <summary>
/// Готовая строка карточки: и цифра, и разбор, откуда она взялась.
/// Считается один раз в VerbResolveSystem и складывается в VerbOffersAttribute.
/// Применение берёт отсюда же — поэтому нельзя сделать то, чего тебе не показали.
/// </summary>
public struct VerbOutcome
{
    public VerbId Verb;
    public string Title;
    public string CostLine;
    public string Breakdown;

    public int Opinion;
    public int Chance;
    public bool IsChanceBased;

    public int GoldCost;
    public int FoodCost;
    public int OpinionOnFail;
    public int TroopsGained;
    public int RivalOpinion;
    public int CommonsOpinion;
    public int CourtOpinion;

    public ConsequenceId OnFail;                                    // провал самого броска
    public System.Collections.Generic.List<VerbConsequence> Consequences;   // реакции черт

    public bool Available;
    public string Blocked;
}
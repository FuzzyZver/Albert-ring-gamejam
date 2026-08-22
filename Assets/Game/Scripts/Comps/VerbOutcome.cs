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
    public int RivalOpinion;
    public int CommonsOpinion;
    public int CourtOpinion;

    public ConsequenceId OnFail;

    public bool Available;
    public string Blocked;
}

/// <summary>Что показать в окошке. Собирает система, вьюха только хранит.</summary>
public struct MetaText
{
    public string Title;
    public string Body;

    public bool IsEmpty => string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Body);
}
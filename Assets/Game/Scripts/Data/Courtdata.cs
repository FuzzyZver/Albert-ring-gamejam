using System.Collections.Generic;

/// <summary>
/// Сгенерированный персонаж. Обычный класс, не SO и не компонент:
/// в SO писать в рантайме нельзя (пачкает ассет в редакторе и молча
/// теряется в билде), а до спавна сущностей ECS ещё нет.
/// </summary>
public class LordData
{
    public int Id = -1;          // -1 = игрок или кандидат
    public string Title;
    public string GivenName;
    public string Epithet;
    public Gender Gender;

    public TraitId TraitA;
    public TraitId TraitB;
    public AmbitionId Ambition;

    public int Troops;
    public int RivalId = -1;     // -1 = соперника нет

    public string FullName => string.IsNullOrEmpty(Epithet)
        ? $"{Title} {GivenName}"
        : $"{Title} {GivenName} {Epithet}";
}

public class CourtData
{
    public int Seed;
    public List<LordData> Lords = new List<LordData>();
    public List<LordData> Candidates = new List<LordData>();   // из кого выбирает игрок
    public LordData Player;                                    // заполняется после выбора
}
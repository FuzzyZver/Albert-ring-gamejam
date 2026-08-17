using System.Collections.Generic;

/// <summary>
/// Чистая функция: сид + конфиги -> двор. Ни одного поля, ни одного обращения
/// к сцене, ни одного Random.Range. Один System.Random на весь забег, сид
/// сохраняется в CourtData и печатается в эпилоге — так баг воспроизводится.
/// </summary>
public static class CourtGenerator
{
    public static CourtData Generate(CharactersConfig chars, BalanceConfig balance, int seed)
    {
        var rng = new System.Random(seed);
        var court = new CourtData { Seed = seed };

        var traits = new Deck<TraitId>(TraitPool(chars), rng);
        var ambitions = new Deck<AmbitionId>(AmbitionPool(chars), rng);
        var maleNames = new Deck<string>(chars.MaleNames, rng);
        var femaleNames = new Deck<string>(chars.FemaleNames, rng);

        court.Player = MakePerson(chars, balance, rng, traits, maleNames, femaleNames);
        court.Player.Id = -1;
        court.Player.Troops = 0;

        for (int i = 0; i < balance.LordsCount; i++)
        {
            var lord = MakePerson(chars, balance, rng, traits, maleNames, femaleNames);
            lord.Id = i;
            lord.Ambition = ambitions.Draw();
            lord.Troops = rng.Next(balance.LordTroopsMin, balance.LordTroopsMax + 1);
            court.Lords.Add(lord);
        }

        AssignRivals(court.Lords, rng);
        return court;
    }

    private static LordData MakePerson(CharactersConfig chars, BalanceConfig balance, System.Random rng,
        Deck<TraitId> traits, Deck<string> maleNames, Deck<string> femaleNames)
    {
        var gender = rng.Next(2) == 0 ? Gender.Male : Gender.Female;

        var traitA = traits.Draw();
        var traitB = traits.Draw();
        for (int guard = 0; traitB == traitA && guard < 8; guard++)
            traitB = traits.Draw();

        var titles = chars.Titles(gender);
        var epithets = chars.Epithets(gender);

        return new LordData
        {
            Gender = gender,
            Title = Pick(titles, rng),
            GivenName = (gender == Gender.Male ? maleNames : femaleNames).Draw(),
            Epithet = rng.Next(100) < balance.EpithetChance ? Pick(epithets, rng) : string.Empty,
            TraitA = traitA,
            TraitB = traitB,
        };
    }

    /// <summary>Разбивает лордов на пары врагов. Нечётный остаётся без соперника — и это хорошо:
    /// в каждом забеге есть один человек, которого не с кем стравить.</summary>
    private static void AssignRivals(List<LordData> lords, System.Random rng)
    {
        var order = Shuffled(lords.ToArray(), rng);
        for (int i = 0; i + 1 < order.Length; i += 2)
        {
            order[i].RivalId = order[i + 1].Id;
            order[i + 1].RivalId = order[i].Id;
        }
    }

    private static TraitId[] TraitPool(CharactersConfig chars)
    {
        var result = new TraitId[chars.Traits.Length];
        for (int i = 0; i < chars.Traits.Length; i++) result[i] = chars.Traits[i].Id;
        return result;
    }

    private static AmbitionId[] AmbitionPool(CharactersConfig chars)
    {
        var result = new AmbitionId[chars.Ambitions.Length];
        for (int i = 0; i < chars.Ambitions.Length; i++) result[i] = chars.Ambitions[i].Id;
        return result;
    }

    private static T Pick<T>(T[] source, System.Random rng) =>
        source == null || source.Length == 0 ? default : source[rng.Next(source.Length)];

    private static T[] Shuffled<T>(T[] source, System.Random rng)
    {
        var copy = (T[])source.Clone();
        for (int i = copy.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }

    /// <summary>Колода вместо броска на каждого: двое лордов не получат одинаковую амбицию,
    /// пока колода не кончилась, и двор всегда выглядит разнообразным.</summary>
    private class Deck<T>
    {
        private readonly T[] _source;
        private readonly System.Random _rng;
        private T[] _current;
        private int _cursor;

        public Deck(T[] source, System.Random rng)
        {
            _source = source ?? new T[0];
            _rng = rng;
            Reshuffle();
        }

        public T Draw()
        {
            if (_source.Length == 0) return default;
            if (_cursor >= _current.Length) Reshuffle();
            return _current[_cursor++];
        }

        private void Reshuffle()
        {
            _current = Shuffled(_source, _rng);
            _cursor = 0;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Концовки. Раньше это были смерти в CharactersConfig; теперь у каждой есть
/// условие, и выбирает подходящую сам конфиг. Чтобы добавить новую концовку,
/// код трогать не нужно: допиши значение в EndingId, запись сюда — и всё.
///
/// Если условиям отвечают несколько, побеждает та, у которой Priority выше.
/// Поэтому пороговые эпилоги осады стоят на нуле, а особые — выше.
/// </summary>
[CreateAssetMenu(fileName = "EndingsConfig", menuName = "Configs/EndingsConfig")]
public class EndingsConfig : ScriptableObject
{
    public EndingDefinition[] Endings = DefaultEndings();

    public int Total => Endings != null ? Endings.Length : 0;

    public EndingDefinition Get(EndingId id) => Array.Find(Endings, e => e.Id == id);

    /// <summary>Лучшая подходящая концовка. Если не подошла ни одна — последняя запись,
    /// чтобы игрок никогда не остался с пустым экраном.</summary>
    public EndingDefinition Select(EndingContext context)
    {
        EndingDefinition best = null;

        for (int i = 0; i < Endings.Length; i++)
        {
            var candidate = Endings[i];
            if (!candidate.Matches(context)) continue;
            if (best == null || candidate.Priority > best.Priority) best = candidate;
        }

        return best ?? (Endings.Length > 0 ? Endings[Endings.Length - 1] : null);
    }

    [ContextMenu("Добавить недостающие записи")]
    private void FillMissing()
    {
        var defaults = DefaultEndings();
        var list = new List<EndingDefinition>(Endings ?? new EndingDefinition[0]);
        int added = 0;

        for (int d = 0; d < defaults.Length; d++)
        {
            if (list.Exists(e => e.Id == defaults[d].Id)) continue;
            list.Add(defaults[d]);
            added++;
        }

        Endings = list.ToArray();
        Debug.Log(added > 0 ? $"{name}: дописано концовок — {added}" : $"{name}: всё на месте", this);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Пересобрать с нуля")]
    private void ResetToDefaults()
    {
        Endings = DefaultEndings();
        Debug.LogWarning($"{name}: концовки перезаписаны значениями из кода.", this);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // ─────────────────────────  СМЕРТИ  ─────────────────────────

    private static EndingDefinition[] DefaultEndings() => new[]
    {
        Death(EndingId.RiotEnding, DeathCause.Riot, "Бунт",
            "Толпа не стала слушать. Ворота открыли изнутри."),
        Death(EndingId.FamineEnding, DeathCause.Famine, "Голод",
            "Амбары стояли пустыми не первую ночь. Голод не спрашивает титула."),
        Death(EndingId.AssassinationEnding, DeathCause.Assassination, "Нож в спину",
            "{lord} вошёл без стука. Это всё, что известно летописцу."),
        Death(EndingId.OverthrowEnding, DeathCause.Overthrow, "Свержение",
            "Лорды сговорились. Кольцо сняли с ещё тёплой руки."),
        Death(EndingId.DuelEnding, DeathCause.Duel, "Поединок",
            "Ты принял вызов {lord}. Летописец отметил, что держался ты достойно ровно два удара."),
        Death(EndingId.AccidentEnding, DeathCause.Accident, "Нелепость", "{detail}"),

        // ─────────────  ПЯТЬ ПОРОГОВ ОСАДЫ  ─────────────

        new EndingDefinition {
            Id = EndingId.SiegeCrushing, Title = "Осада снята", Victory = true,
            ForVictory = true, MinForceLeftPercent = 70,
            ChronicleLine = "Они ушли на третий день, оставив обоз и половину лестниц. " +
                            "Кольцо на твоей руке, гарнизон почти цел, и летописец впервые пишет без иронии.",
        },
        new EndingDefinition {
            Id = EndingId.SiegeHeld, Title = "Ты выстоял", Victory = true,
            ForVictory = true, MinForceLeftPercent = 35, MaxForceLeftPercent = 69,
            ChronicleLine = "Стены выдержали. Людей на них осталось меньше, чем было, но они остались. " +
                            "Этого хватило.",
        },
        new EndingDefinition {
            Id = EndingId.SiegePyrrhic, Title = "Победа некому праздновать", Victory = true,
            ForVictory = true, MaxForceLeftPercent = 34,
            ChronicleLine = "Осада снята. На стенах стоят четырнадцать человек и ты. " +
                            "Летописец записал победу и надолго замолчал.",
        },
        new EndingDefinition {
            Id = EndingId.SiegeFallen, Title = "Замок пал", ForDefeat = true, MaxEnemyLeftPercent = 45,
            ChronicleLine = "Ты продал замок дороже, чем он стоил. Их полководец приказал похоронить тебя с кольцом.",
        },
        new EndingDefinition {
            Id = EndingId.SiegeMassacre, Title = "Разгром", ForDefeat = true, MinEnemyLeftPercent = 46,
            ChronicleLine = "Ворота держались меньше часа. Летописец не дописал предложение.",
        },

        // ─────────────  ОСОБЫЕ, ПО ФЛАГАМ  ─────────────

        new EndingDefinition {
            Id = EndingId.LonelyCrown, Title = "Одинокая корона", Victory = true, Priority = 10,
            ForVictory = true, MaxLords = 0,
            ChronicleLine = "Ни один лорд не приехал. Ты стоял на стене один и выстоял один. " +
                            "Теперь тебя боятся сильнее, чем осаждавших.",
        },
        new EndingDefinition {
            Id = EndingId.SaintKing, Title = "Народный государь", Victory = true, Priority = 12,
            ForVictory = true, MinCommons = 55,
            ChronicleLine = "На стены вышла деревня. С вилами, с топорами, с чем было. " +
                            "Летописец пишет, что таких осад он не помнит.",
        },
        new EndingDefinition {
            Id = EndingId.WidowedCrown, Title = "Вдовье кольцо", Victory = true, Priority = 14,
            ForVictory = true, NeedsDeadLover = true,
            ChronicleLine = "Осада снята. Того, ради кого стоило её снимать, на стенах не было. " +
                            "Кольцо оказалось тяжелее, чем помнилось.",
        },
    };

    private static EndingDefinition Death(EndingId id, DeathCause cause, string title, string line) =>
        new EndingDefinition { Id = id, Cause = cause, Title = title, ChronicleLine = line, Priority = 100 };
}

// ─────────────────────────  ОПРЕДЕЛЕНИЯ  ─────────────────────────

/// <summary>Всё, что известно о забеге на момент конца. Собирается один раз
/// и передаётся в EndingsConfig.Select.</summary>
public struct EndingContext
{
    public DeathCause Cause;
    public bool Victory;
    public int Day;
    public int ForceLeftPercent;    // сколько наших осталось от начала осады
    public int EnemyLeftPercent;
    public int LordsPresent;
    public int Commons;
    public bool HasLover;
    public bool LoverDead;
}

[Serializable]
public class EndingDefinition
{
    public EndingId Id;
    public string Title;
    [TextArea(2, 5)] public string ChronicleLine;   // слоты {lord} и {detail}
    public bool Victory;

    [Header("Кто выше — тот и выпадет")]
    public int Priority;

    [Header("Условия. Смерть задаётся причиной, осада — порогами")]
    public DeathCause Cause = DeathCause.None;
    public bool ForVictory;
    public bool ForDefeat;

    public int MinForceLeftPercent = 0;
    public int MaxForceLeftPercent = 100;
    public int MinEnemyLeftPercent = 0;
    public int MaxEnemyLeftPercent = 100;

    public int MinLords = 0;
    public int MaxLords = 99;
    public int MinCommons = -100;
    public int MaxCommons = 100;

    public bool NeedsLover;
    public bool NeedsDeadLover;

    public bool Matches(EndingContext context)
    {
        if (Cause != DeathCause.None) return context.Cause == Cause;
        if (context.Cause != DeathCause.None) return false;

        if (ForVictory && !context.Victory) return false;
        if (ForDefeat && context.Victory) return false;

        if (context.ForceLeftPercent < MinForceLeftPercent) return false;
        if (context.ForceLeftPercent > MaxForceLeftPercent) return false;
        if (context.EnemyLeftPercent < MinEnemyLeftPercent) return false;
        if (context.EnemyLeftPercent > MaxEnemyLeftPercent) return false;

        if (context.LordsPresent < MinLords || context.LordsPresent > MaxLords) return false;
        if (context.Commons < MinCommons || context.Commons > MaxCommons) return false;

        if (NeedsLover && !context.HasLover) return false;
        if (NeedsDeadLover && !context.LoverDead) return false;

        return true;
    }
}
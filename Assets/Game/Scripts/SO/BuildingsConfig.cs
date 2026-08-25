using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Постройки и то, что в замке можно устроить. Отдельный ассет по тому же правилу,
/// что и EventsConfig: одна тема — один конфиг. Числа и текст постройки лежат рядом,
/// потому что порознь они бессмысленны: «+6 золота» без «Рыночная площадь» не читается.
///
/// Действие описывается через ChoiceDefinition — тот же тип, что у просителей
/// и вечерних событий, и применяет его та же ChoiceEffectSystem.
/// </summary>
[CreateAssetMenu(fileName = "BuildingsConfig", menuName = "Configs/BuildingsConfig")]
public class BuildingsConfig : ScriptableObject
{
    public int MaxLevel = 3;

    public BuildingDefinition[] Buildings = DefaultBuildings();
    public CastleActionDefinition[] Actions = DefaultActions();

    public BuildingDefinition GetBuilding(BuildingId id) => Array.Find(Buildings, b => b.Id == id);
    public CastleActionDefinition GetAction(CastleActionId id) => Array.Find(Actions, a => a.Id == id);

    [ContextMenu("Добавить недостающие записи")]
    private void FillMissing()
    {
        int added = 0;
        Buildings = Merge(Buildings, DefaultBuildings(), b => (int)b.Id, ref added);
        Actions = Merge(Actions, DefaultActions(), a => (int)a.Id, ref added);

        Debug.Log(added > 0 ? $"{name}: дописано записей — {added}" : $"{name}: всё на месте", this);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Пересобрать с нуля")]
    private void ResetToDefaults()
    {
        Buildings = DefaultBuildings();
        Actions = DefaultActions();
        Debug.LogWarning($"{name}: постройки перезаписаны значениями из кода.", this);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private static T[] Merge<T>(T[] current, T[] defaults, Func<T, int> id, ref int added)
    {
        var list = new List<T>(current ?? new T[0]);

        for (int d = 0; d < defaults.Length; d++)
        {
            bool exists = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (id(list[i]) != id(defaults[d])) continue;
                exists = true;
                break;
            }

            if (exists) continue;
            list.Add(defaults[d]);
            added++;
        }

        return list.ToArray();
    }

    // ─────────────────────────  ПОСТРОЙКИ  ─────────────────────────
    // Первый уровень встаёт за день (0 дней стройки), второй — за сутки,
    // третий — за двое. Всё в BuildDays, крутится в инспекторе.

    private static BuildingDefinition[] DefaultBuildings() => new[]
    {
        new BuildingDefinition {
            Id = BuildingId.Market, EmptyLabel = "Пустырь у ворот",
            Description = "Торговля кормит казну и злит тех, кто привык кормиться сам.",
            Tiers = new[] {
                T("Торговые ряды", 20, 0, 0, goldPerDay: 3,
                    effects: new[] { E(TraitId.Greedy, -1) }),
                T("Рыночная площадь", 35, 0, 1, goldPerDay: 6,
                    effects: new[] { E(TraitId.Greedy, -2) }),
                T("Гостиный двор", 55, 0, 2, goldPerDay: 10, commons: 1,
                    effects: new[] { E(TraitId.Greedy, -3) }),
            },
        },

        new BuildingDefinition {
            Id = BuildingId.Walls, EmptyLabel = "Старый вал",
            Description = "Единственное, что будет иметь значение на двенадцатый день.",
            Tiers = new[] {
                T("Частокол", 15, 0, 0, siege: 8),
                T("Каменная стена", 30, 10, 1, siege: 18),
                T("Стены и башни", 50, 20, 2, siege: 32),
            },
        },

        new BuildingDefinition {
            Id = BuildingId.Temple, EmptyLabel = "Место под храм",
            Description = "Крестьяне это видят. Набожные лорды — тем более.",
            Tiers = new[] {
                T("Часовня", 18, 0, 0, commons: 2,
                    effects: new[] { E(TraitId.Pious, 2) }),
                T("Церковь", 32, 0, 1, commons: 4,
                    effects: new[] { E(TraitId.Pious, 3) }),
                T("Собор", 55, 0, 2, commons: 6,
                    effects: new[] { E(TraitId.Pious, 4) }),
            },
        },

        new BuildingDefinition {
            Id = BuildingId.Brothel, EmptyLabel = "Пустые склады",
            Description = "Двор доволен. Приход — нет.",
            Tiers = new[] {
                T("Весёлый двор", 15, 0, 0, court: 1,
                    effects: new[] { E(TraitId.Pious, -3), E(TraitId.Drunkard, 1), E(TraitId.Lustful, 1) }),
                T("Публичные дома", 28, 0, 1, court: 2,
                    effects: new[] { E(TraitId.Pious, -5), E(TraitId.Drunkard, 2), E(TraitId.Lustful, 2) }),
                T("Квартал утех", 45, 0, 2, court: 3, commons: -1,
                    effects: new[] { E(TraitId.Pious, -8), E(TraitId.Drunkard, 3), E(TraitId.Lustful, 3) }),
            },
        },
    };

    // ─────────────────────────  ДЕЙСТВИЯ  ─────────────────────────

    private static CastleActionDefinition[] DefaultActions() => new[]
    {
        new CastleActionDefinition {
            Id = CastleActionId.Feast, Title = "Объявить пир",
            Description = "Столы вынесут завтра вечером. Во что это обойдётся — увидишь тогда же.",
            CooldownDays = 2, QueuesFeast = true,
            Effect = new ChoiceDefinition { Label = "Объявить пир", Result = "Гонцы поехали звать гостей." },
        },

        new CastleActionDefinition {
            Id = CastleActionId.TempleService, Title = "Служба в храме",
            Description = "Молебен за здравие государя. Слышно далеко.",
            CooldownDays = 2, SiegeBonus = 6,
            NeedsBuilding = true, RequiredBuilding = BuildingId.Temple, RequiredLevel = 1,
            Effect = new ChoiceDefinition {
                Label = "Отслужить", Result = "Пели долго. Крестьяне остались до конца, лорды — почти все.",
                Gold = -8, CommonsOpinion = 10, CourtOpinion = 4,
            },
        },

        new CastleActionDefinition {
            Id = CastleActionId.HireMercenaries, Title = "Вербовка наёмников",
            Description = "Чужие копья на стенах. Лорды считают это оскорблением, но копья есть копья.",
            CooldownDays = 1,
            Effect = new ChoiceDefinition {
                Label = "Нанять", Result = "Десять чужих людей встали на стены и потребовали ужин.",
                Gold = -30, Garrison = 10, CourtOpinion = -4,
            },
        },
    };

    /// <summary>Первые три — цена и срок, дальше именованные — что даёт за ночь.
    /// Названия у цены и у дохода нарочно разные: иначе «gold» читается двояко.</summary>
    private static BuildingTier T(string title, int costGold, int costFood, int buildDays,
        int goldPerDay = 0, int foodPerDay = 0, int commons = 0, int court = 0, int siege = 0,
        TraitOpinionEffect[] effects = null)
    {
        return new BuildingTier
        {
            Title = title,
            GoldCost = costGold,
            FoodCost = costFood,
            BuildDays = buildDays,
            GoldPerDay = goldPerDay,
            FoodPerDay = foodPerDay,
            CommonsPerDay = commons,
            CourtPerDay = court,
            SiegeDefence = siege,
            LordEffects = effects ?? new TraitOpinionEffect[0],
        };
    }

    private static TraitOpinionEffect E(TraitId trait, int opinion) =>
        new TraitOpinionEffect { Trait = trait, Opinion = opinion };
}

// ─────────────────────────  ОПРЕДЕЛЕНИЯ  ─────────────────────────

/// <summary>Ночная прибавка к мнению лордов с определённой чертой.
/// Invert значит «всем, кроме носителей».</summary>
[Serializable]
public struct TraitOpinionEffect
{
    public TraitId Trait;
    public bool Invert;
    public int Opinion;

    public bool Applies(TraitsAttribute traits) => traits.Has(Trait) != Invert;
}

[Serializable]
public class BuildingTier
{
    public string Title;
    public int GoldCost;
    public int FoodCost;
    public int BuildDays;    // 0 — встанет к утру

    [Header("Каждую ночь")]
    public int GoldPerDay;
    public int FoodPerDay;
    public int CommonsPerDay;
    public int CourtPerDay;
    public TraitOpinionEffect[] LordEffects = new TraitOpinionEffect[0];

    [Header("Осада")]
    public int SiegeDefence;

    /// <summary>Человеческое описание бонусов — строится из чисел,
    /// чтобы карточка не врала, когда ты подкрутишь баланс.</summary>
    public string BonusLine(CharactersConfig chars)
    {
        var text = new StringBuilder();
        Add(text, GoldPerDay, "золота за ночь");
        Add(text, FoodPerDay, "пищи за ночь");
        Add(text, CommonsPerDay, "крестьянам за ночь");
        Add(text, CourtPerDay, "всем лордам за ночь");
        Add(text, SiegeDefence, "к обороне");

        if (LordEffects == null) return text.ToString();

        for (int i = 0; i < LordEffects.Length; i++)
        {
            var effect = LordEffects[i];
            if (effect.Opinion == 0) continue;

            string trait = chars != null ? chars.TraitTitle(effect.Trait, Gender.Male) : effect.Trait.ToString();
            Add(text, effect.Opinion, effect.Invert ? "всем кроме: " + trait : trait);
        }

        return text.ToString();
    }

    public string CostLine()
    {
        var text = new StringBuilder();
        if (GoldCost > 0) text.Append(GoldCost).Append(" золота");
        if (FoodCost > 0) { if (text.Length > 0) text.Append(" · "); text.Append(FoodCost).Append(" пищи"); }
        if (BuildDays > 0) { if (text.Length > 0) text.Append(" · "); text.Append(BuildDays).Append(BuildDays == 1 ? " день" : " дня"); }
        return text.Length > 0 ? text.ToString() : "даром";
    }

    public bool CanAfford(int gold, int food) => gold >= GoldCost && food >= FoodCost;

    private static void Add(StringBuilder text, int value, string name)
    {
        if (value == 0) return;
        if (text.Length > 0) text.Append(" · ");
        text.Append(value > 0 ? "+" : string.Empty).Append(value).Append(' ').Append(name);
    }
}

[Serializable]
public class BuildingDefinition
{
    public BuildingId Id;
    public string EmptyLabel;               // что написано на пустой булавке
    [TextArea(1, 3)] public string Description;
    public BuildingTier[] Tiers = new BuildingTier[0];

    public BuildingTier Tier(int level) =>
        Tiers != null && level >= 1 && level <= Tiers.Length ? Tiers[level - 1] : null;

    public int MaxLevel => Tiers != null ? Tiers.Length : 0;

    /// <summary>Название на текущем уровне. На нуле — что здесь можно построить.</summary>
    public string TitleAt(int level)
    {
        var tier = Tier(level);
        return tier != null ? tier.Title : EmptyLabel;
    }
}

[Serializable]
public class CastleActionDefinition
{
    public CastleActionId Id;
    public string Title;
    [TextArea(1, 3)] public string Description;

    public ChoiceDefinition Effect = new ChoiceDefinition();

    [Header("Сверх обычных эффектов")]
    public int SiegeBonus;      // копится до осады
    public bool QueuesFeast;    // ставит пир на завтрашний вечер

    [Header("Условия")]
    public int CooldownDays;
    public bool NeedsBuilding;
    public BuildingId RequiredBuilding;
    public int RequiredLevel = 1;
}
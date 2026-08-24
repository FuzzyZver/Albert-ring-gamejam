using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Тексты и решения. Отдельным ассетом от CharactersConfig намеренно:
/// там правила характеров, здесь — сценки, и растут они с разной скоростью.
///
/// Просители и вечерние события устроены одинаково: заголовок, текст со слотами
/// и до трёх вариантов. Поэтому вариант описывается одним типом ChoiceDefinition,
/// а применяет его одна система на всех — ChoiceEffectSystem.
/// </summary>
[CreateAssetMenu(fileName = "EventsConfig", menuName = "Configs/EventsConfig")]
public class EventsConfig : ScriptableObject
{
    public PetitionDefinition[] Petitions = DefaultPetitions();
    public EveningEventDefinition[] EveningEvents = DefaultEveningEvents();

    public PetitionDefinition GetPetition(PetitionId id) => Array.Find(Petitions, p => p.Id == id);
    public EveningEventDefinition GetEvening(EveningEventId id) => Array.Find(EveningEvents, e => e.Id == id);

    /// <summary>Подстановка в слоты. {player} — ты, {lord} — привязанный лорд,
    /// {petitioner} — кто стоит перед троном.</summary>
    public string Fill(string text, string player, string lord, string petitioner)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        return text
            .Replace("{player}", player ?? string.Empty)
            .Replace("{lord}", lord ?? string.Empty)
            .Replace("{petitioner}", petitioner ?? string.Empty);
    }

    [ContextMenu("Добавить недостающие записи")]
    private void FillMissing()
    {
        int added = 0;
        Petitions = Merge(Petitions, DefaultPetitions(), p => (int)p.Id, ref added);
        EveningEvents = Merge(EveningEvents, DefaultEveningEvents(), e => (int)e.Id, ref added);

        Debug.Log(added > 0 ? $"{name}: дописано записей — {added}" : $"{name}: всё на месте", this);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Пересобрать с нуля")]
    private void ResetToDefaults()
    {
        Petitions = DefaultPetitions();
        EveningEvents = DefaultEveningEvents();
        Debug.LogWarning($"{name}: тексты перезаписаны значениями из кода.", this);
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

    // ─────────────────────────  ПРОСИТЕЛИ  ─────────────────────────

    private static PetitionDefinition[] DefaultPetitions() => new[]
    {
        new PetitionDefinition {
            Id = PetitionId.LandlessKnight, Petitioner = "Рыцарь без надела",
            Text = "Я служил твоему отцу под Аскелоном, {player}. Земли у меня нет, есть меч и конь. Дай место при дворе.",
            Choices = new[] {
                C("Принять и снарядить", "Рыцарь получил койку и овса для коня. Двор ворчит, но копий стало больше.",
                    gold: -12, garrison: 4, court: -3),
                C("Отказать", "Он поклонился и ушёл не оборачиваясь. Лорды заметили, как ты обошёлся со старым служакой.",
                    court: -8),
            },
        },

        new PetitionDefinition {
            Id = PetitionId.BanditsInTheWood, Petitioner = "Крестьянин из Броды",
            Text = "В лесу разбойники, третью неделю. Дай солдат, {player}, иначе зерно до тебя не доедет.",
            Choices = new[] {
                C("Дать троих", "Троих хватило. Зерно доехало, а деревня запомнила.",
                    food: 4, garrison: -3, commons: 18),
                C("Справляйтесь сами", "Он молчал дольше, чем следовало, и ушёл.",
                    commons: -14),
                C("Пусть платят разбойникам", "Ты назвал это налогом. Крестьяне назвали иначе.",
                    gold: 8, commons: -22, consequence: ConsequenceId.TurnipToTheFace, chance: 25),
            },
        },

        new PetitionDefinition {
            Id = PetitionId.BurnedMill, Petitioner = "Мельник",
            Text = "Мельница у брода сгорела дотла. Сама она гореть не умеет, {player}.",
            Choices = new[] {
                C("Отстроить", "К концу недели колесо снова скрипело. Мука пошла в замок.",
                    gold: -18, food: 6, commons: 14),
                C("Не до мельницы сейчас", "Мельник пошёл искать правду в соседнем графстве.",
                    food: -4, commons: -10),
            },
        },

        new PetitionDefinition {
            Id = PetitionId.LowerTheToll, Petitioner = "Городской голова",
            Text = "Город просит снять пошлину на неделю. Осада всё равно съест всё, что мы накопим.",
            Choices = new[] {
                C("Снять на неделю", "Купцы вздохнули. Казна тоже, но по другому поводу.",
                    gold: -10, commons: 14),
                C("Оставить как есть", "Голова кивнул. Кивок был сухой.",
                    commons: -6),
                C("Наоборот, поднять вдвое", "Деньги пришли быстро. Слухи — ещё быстрее.",
                    gold: 16, commons: -20, consequence: ConsequenceId.SpreadRumor, chance: 30),
            },
        },

        new PetitionDefinition {
            Id = PetitionId.AbsentFromMass, Petitioner = "Брат-настоятель", NeedsLord = true,
            Text = "{lord} не был на службе целый месяц. Люди смотрят на это и делают выводы, {player}.",
            Choices = new[] {
                C("Пусть кается прилюдно", "{lord} стоял на коленях на площади. Площадь была довольна.",
                    commons: 12, lord: -18),
                C("Оставь его в покое", "Настоятель поджал губы. {lord} узнал и оценил.",
                    commons: -10, lord: 8),
                C("Это моё дело, не твоё", "Двор одобрил. Приход — нет.",
                    commons: -14, court: 4),
            },
        },

        new PetitionDefinition {
            Id = PetitionId.SoldiersWidow, Petitioner = "Вдова",
            Text = "Мой муж стоял на твоих стенах, {player}. У меня трое детей и пустой ларь.",
            Choices = new[] {
                C("Дать зерна", "Ларь наполнился. К вечеру об этом знала вся деревня.",
                    food: -8, commons: 14),
                C("Казна пуста", "Она не спорила. Это было хуже, чем если бы спорила.",
                    commons: -12),
            },
        },

        new PetitionDefinition {
            Id = PetitionId.MercenaryCaptain, Petitioner = "Капитан наёмников",
            Text = "Двенадцать копий, все с опытом. Цена известна, торг неуместен.",
            Choices = new[] {
                C("Нанять", "Двенадцать чужих людей встали на стены. Лорды считают это оскорблением.",
                    gold: -25, garrison: 12, court: -4),
                C("Дорого", "Капитан пожал плечами и поехал к соседям."),
            },
        },

        new PetitionDefinition {
            Id = PetitionId.Informer, Petitioner = "Доносчик", NeedsLord = true,
            Text = "{lord} говорил о тебе за столом, государь. Слова были нехорошие. Я запомнил дословно.",
            Choices = new[] {
                C("Поверить", "Ты поверил человеку, которого видишь впервые. Двор это заметил.",
                    court: -6, lord: -20),
                C("Прогнать доносчика", "Ты дал ему монету и указал на дверь. Лорды одобрили.",
                    gold: -3, court: 6),
                C("Заплатить за молчание", "Он взял и обещал молчать. Обещал.",
                    gold: -10, consequence: ConsequenceId.SpreadRumor, chance: 40),
            },
        },
    };

    // ─────────────────────────  ВЕЧЕРА  ─────────────────────────

    private static EveningEventDefinition[] DefaultEveningEvents() => new[]
    {
        new EveningEventDefinition {
            Id = EveningEventId.QuietEvening, Title = "Вечер", Weight = 6,
            Text = "Свечи догорели, никто не пришёл. Летописец записал «день без происшествий» и, кажется, был разочарован.",
            Choices = new ChoiceDefinition[0],
        },

        new EveningEventDefinition {
            Id = EveningEventId.Feast, Title = "Пир", Weight = 0, NeedsLord = true,
            Text = "Столы вынесли во двор. {lord} уже требует вторую бочку и рассказывает, как брал Аскелон.",
            Choices = new[] {
                C("Пусть льётся", "Двор гулял до третьих петухов. Погреб этого не пережил.",
                    food: -10, court: 12, consequence: ConsequenceId.GetDrunk, chance: 30),
                C("Скромно, но достойно", "Все разошлись сытыми и слегка разочарованными.",
                    food: -5, court: 5),
            },
        },

        new EveningEventDefinition {
            Id = EveningEventId.DrunkenNight, Title = "Погреб", Weight = 10,
            NeedsPlayerTrait = true, PlayerTrait = TraitId.Drunkard,
            Text = "Ты спустился за одной кружкой. Погреб оказался глубже, чем помнилось.",
            Choices = new[] {
                C("Ещё одну", "Утром ты не смог вспомнить, кому и что обещал.",
                    commons: -4, consequence: ConsequenceId.ConfessDrunkenly, chance: 35),
                C("Хватит", "Ты поставил кружку и поднялся наверх. Это стоило усилий."),
            },
        },

        new EveningEventDefinition {
            Id = EveningEventId.ProphecyAtTable, Title = "Пророчество", Weight = 10,
            NeedsCourtTrait = true, CourtTrait = TraitId.Obsessed, NeedsLord = true,
            Text = "{lord} встал посреди ужина и объявил, что видел твою смерть. Подробно. С датами.",
            Choices = new[] {
                C("Выслушать до конца", "Ты слушал час. {lord} счёл тебя единственным разумным человеком в замке.",
                    court: -5, lord: 20),
                C("Вывести его", "Его вывели под руки. Он кричал что-то про молнию.",
                    court: 5, lord: -18),
                C("Посмеяться", "Смеялся весь стол. {lord} смеялся громче всех, и это было зря.",
                    commons: 5, lord: -10, consequence: ConsequenceId.LordDiesLaughing, chance: 5),
            },
        },

        new EveningEventDefinition {
            Id = EveningEventId.LoversInTheHall, Title = "Коридор", Weight = 12,
            NeedsLover = true, NeedsLord = true,
            Text = "{lord} ждал тебя у лестницы. Ждал, как выяснилось, не только {lord}.",
            Choices = new[] {
                C("Пройти мимо", "Ты прошёл мимо. Оба это запомнили.",
                    lord: -12),
                C("Остаться", "Ты остался. К утру об этом знали все, кто не спал.",
                    court: -8, lord: 15, consequence: ConsequenceId.Scandal, chance: 30),
            },
        },

        new EveningEventDefinition {
            Id = EveningEventId.PeasantsAtTheGate, Title = "У ворот", Weight = 14,
            MaxCommons = -20,
            Text = "У ворот человек тридцать. Пока молча. Молчат они с самого полудня.",
            Choices = new[] {
                C("Выйти и говорить", "Ты вышел безоружным. Это оказалось верным решением. Почти.",
                    commons: 16, consequence: ConsequenceId.TurnipToTheFace, chance: 20),
                C("Раздать зерно", "Зерно кончилось раньше, чем люди. Но они ушли.",
                    food: -10, commons: 20),
                C("Спустить стражу", "Ворота остались целы. Больше ничего хорошего не случилось.",
                    garrison: -1, commons: -20, court: 4),
            },
        },

        new EveningEventDefinition {
            Id = EveningEventId.RumorSpreads, Title = "Слух", Weight = 8, MinDay = 3,
            Text = "К вечеру по замку пошёл слух. Про тебя, разумеется. Подробностей никто не знает, но все уверены.",
            Choices = new[] {
                C("Не заметить", "Слух пожил своей жизнью и оброс деталями.",
                    court: -6),
                C("Найти источник", "Источник нашёлся и стоил дороже, чем сам слух.",
                    gold: -8, court: 4),
            },
        },
    };

    private static ChoiceDefinition C(string label, string result,
        int gold = 0, int food = 0, int garrison = 0,
        int commons = 0, int court = 0, int lord = 0,
        ConsequenceId consequence = ConsequenceId.None, int chance = 100) =>
        new ChoiceDefinition
        {
            Label = label,
            Result = result,
            Gold = gold,
            Food = food,
            Garrison = garrison,
            CommonsOpinion = commons,
            CourtOpinion = court,
            LordOpinion = lord,
            Consequence = consequence,
            ConsequenceChance = chance,
        };
}

// ─────────────────────────  ОПРЕДЕЛЕНИЯ  ─────────────────────────

/// <summary>Один вариант ответа. Отрицательные Gold/Food/Garrison — это цена:
/// если платить нечем, кнопка гаснет с причиной.</summary>
[Serializable]
public class ChoiceDefinition
{
    public string Label;
    [TextArea(2, 3)] public string Result;

    [Header("Ресурсы: минус — цена")]
    public int Gold;
    public int Food;
    public int Garrison;

    [Header("Мнения")]
    public int CommonsOpinion;
    public int CourtOpinion;
    public int LordOpinion;   // лорду из слота {lord}

    [Header("Последствие")]
    public ConsequenceId Consequence;
    [Range(0, 100)] public int ConsequenceChance;

    public bool CanAfford(int gold, int food, int garrison) =>
        gold + Gold >= 0 && food + Food >= 0 && garrison + Garrison >= 0;

    public string Missing(int gold, int food, int garrison)
    {
        if (gold + Gold < 0) return "не хватает золота";
        if (food + Food < 0) return "не хватает пищи";
        if (garrison + Garrison < 0) return "некого послать";
        return string.Empty;
    }

    /// <summary>Короткая строка под кнопкой: во что это обойдётся и что даст.</summary>
    public string Hint()
    {
        var text = new StringBuilder();
        Add(text, Gold, "золота");
        Add(text, Food, "пищи");
        Add(text, Garrison, "копий");
        Add(text, CommonsOpinion, "крестьяне");
        Add(text, CourtOpinion, "двор");
        return text.ToString();
    }

    private static void Add(StringBuilder text, int value, string name)
    {
        if (value == 0) return;
        if (text.Length > 0) text.Append(" · ");
        text.Append(value > 0 ? "+" : string.Empty).Append(value).Append(' ').Append(name);
    }

    public int RealChance => ConsequenceChance <= 0 ? 100 : ConsequenceChance;
}

[Serializable]
public class PetitionDefinition
{
    public PetitionId Id;
    public string Petitioner;              // кто стоит перед троном
    [TextArea(2, 4)] public string Text;   // слоты {player}, {lord}, {petitioner}
    public bool NeedsLord;                 // привязать случайного лорда к слоту {lord}
    public ChoiceDefinition[] Choices = new ChoiceDefinition[0];
}

[Serializable]
public class EveningEventDefinition
{
    public EveningEventId Id;
    public string Title;
    [TextArea(2, 4)] public string Text;
    public ChoiceDefinition[] Choices = new ChoiceDefinition[0];

    [Header("Когда может случиться. Weight 0 = только по прямому вызову")]
    public int Weight = 10;
    public int MinDay = 1;
    public bool NeedsLord;
    public bool NeedsLover;
    public bool NeedsPlayerTrait;
    public TraitId PlayerTrait;
    public bool NeedsCourtTrait;
    public TraitId CourtTrait;
    public int MaxCommons = 100;   // событие только когда крестьяне не злее этого
}
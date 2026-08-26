using System.Text;
using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Конец забега. Ловит и смерть, и победу, но срабатывает ровно один раз:
/// в одну ночь может сойтись и голод, и осада, а эпилог должен быть один.
/// </summary>
public class RunEndSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<DeathEvent> _deaths;
    private EcsFilter<VictoryEvent> _victories;

    private EcsFilter<RunFlag, CalendarAttribute, TreasuryAttribute>.Exclude<RunOverFlag> _runs;
    private EcsFilter<LordFlag, LordIdAttribute, PersonAttribute, OpinionAttribute> _lords;
    private EcsFilter<LordFlag, LoverFlag> _lovers;

    private readonly StringBuilder _text = new StringBuilder();

    public void Run()
    {
        foreach (var i in _deaths)
        {
            var death = _deaths.Get1(i);
            End(false, death.Cause, death.KillerLordId, death.Detail, 0);
            break;
        }

        foreach (var i in _victories)
        {
            End(true, DeathCause.None, -1, string.Empty, _victories.Get1(i).Defence);
            break;
        }
    }

    private void End(bool victory, DeathCause cause, int killerLordId, string detail, int defence)
    {
        var balance = GameConfig.BalanceConfig;

        foreach (var r in _runs)
        {
            var entity = _runs.GetEntity(r);
            if (entity.Has<RunOverFlag>()) continue;   // фильтр обновится только к концу кадра

            ref var calendar = ref _runs.Get2(r);
            ref var treasury = ref _runs.Get3(r);
            ref var end = ref entity.Get<RunEndAttribute>();

            end.Victory = victory;
            end.Cause = cause;
            end.KillerLordId = killerLordId;
            end.Detail = detail;
            end.Day = calendar.Day;
            end.Defence = defence;
            end.SiegeStrength = balance.SiegeEnemyForce;

            entity.Get<RunOverFlag>();

            ref var siege = ref entity.Get<SiegeAttribute>();
            var definition = GameConfig.EndingsConfig.Select(Context(cause, victory, calendar.Day, siege, entity));

            end.Ending = definition != null ? definition.Id : default;
            end.FirstTime = definition != null && RealtimeData.Unlock(definition.Id);

            string title = definition != null ? definition.Title : cause.ToString();
            string line = definition != null ? definition.ChronicleLine : string.Empty;

            line = line
                .Replace("{lord}", LordName(killerLordId))
                .Replace("{detail}", detail ?? string.Empty)
                .Replace("{day}", calendar.Day.ToString());

            UI.Epilogue.SetEndings(RealtimeData.UnlockedCount, GameConfig.EndingsConfig.Total, end.FirstTime);
            UI.Epilogue.Show(victory, title, line,
                Summary(calendar.Day, balance, treasury, defence, victory),
                CourtLines(balance),
                entity.Get<RngAttribute>().Seed);

            _world.NewEntity().Get<ChronicleEvent>().Line = line;
            _world.NewEntity().Get<ChangeScreenEvent>().Target = ScreenId.Epilogue;
        }
    }

    /// <summary>Всё, по чему EndingsConfig выберет концовку. Проценты считаются
    /// от начала осады, поэтому «выстоял чудом» и «выстоял не заметив» — разные концовки.</summary>
    private EndingContext Context(DeathCause cause, bool victory, int day, SiegeAttribute siege, EcsEntity run)
    {
        int ourStart = Mathf.Max(1, siege.OurStart);
        int enemyStart = Mathf.Max(1, siege.EnemyStart);

        bool hasLover = false;
        foreach (var _ in _lovers) hasLover = true;

        return new EndingContext
        {
            Cause = cause,
            Victory = victory,
            Day = day,
            ForceLeftPercent = siege.OurForce * 100 / ourStart,
            EnemyLeftPercent = siege.EnemyForce * 100 / enemyStart,
            LordsPresent = siege.LordsPresent,
            Commons = run.Get<CommonsAttribute>().Opinion,
            HasLover = hasLover,
            LoverDead = LoverDied(),
        };
    }

    /// <summary>Любовник был, но до осады не дожил — отдельная концовка.</summary>
    private bool LoverDied()
    {
        foreach (var i in _lords)
        {
            var lord = _lords.GetEntity(i);
            if (lord.Has<LoverFlag>() && lord.Has<DeadFlag>()) return true;
        }

        return false;
    }

    private string Summary(int day, BalanceConfig balance, TreasuryAttribute treasury, int defence, bool victory)
    {
        _text.Length = 0;
        _text.Append("Ты продержался ").Append(day).Append(" из ").Append(balance.DaysUntilSiege).AppendLine(" дней.");
        _text.Append("Осталось: ").Append(treasury.Gold).Append(" золота, ")
             .Append(treasury.Food).Append(" пищи, ").Append(treasury.Garrison).AppendLine(" копий гарнизона.");

        if (defence > 0 || victory)
            _text.Append("Стены защищали ").Append(defence).Append(" против ").Append(balance.SiegeEnemyForce).Append('.');

        return _text.ToString();
    }

    /// <summary>Кто пришёл бы на осаду, а кто нет. Половина смысла эпилога — увидеть,
    /// на ком именно ты сэкономил действие.</summary>
    private string CourtLines(BalanceConfig balance)
    {
        _text.Length = 0;

        foreach (var i in _lords)
        {
            var entity = _lords.GetEntity(i);
            int opinion = _lords.Get4(i).Value;

            int threshold = entity.Has<LoverFlag>() ? balance.LoverComeAtOpinion : balance.TroopsComeAtOpinion;
            string verdict =
                entity.Has<DeadFlag>() ? "мёртв" :
                entity.Has<LeftCourtFlag>() ? "уехал" :
                opinion >= threshold ? "пришёл бы" : "не пришёл бы";

            if (_text.Length > 0) _text.AppendLine();
            _text.Append(_lords.Get3(i).FullName).Append(" — ")
                 .Append(opinion > 0 ? "+" + opinion : opinion.ToString())
                 .Append(", ").Append(verdict);
        }

        return _text.ToString();
    }

    private string LordName(int lordId)
    {
        if (lordId < 0) return "Некто";

        foreach (var i in _lords)
            if (_lords.Get2(i).Value == lordId)
                return _lords.Get3(i).FullName;

        return "Некто";
    }
}
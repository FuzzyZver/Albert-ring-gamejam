using System.Text;
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
        var chars = GameConfig.CharactersConfig;
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
            end.SiegeStrength = balance.SiegeStrength;

            entity.Get<RunOverFlag>();

            var definition = chars.GetDeath(cause);
            string title = definition != null ? definition.Title : cause.ToString();
            string line = definition != null ? definition.ChronicleLine : string.Empty;

            line = line
                .Replace("{lord}", LordName(killerLordId))
                .Replace("{detail}", detail ?? string.Empty)
                .Replace("{day}", calendar.Day.ToString());

            UI.Epilogue.Show(victory, title, line,
                Summary(calendar.Day, balance, treasury, defence, victory),
                CourtLines(balance),
                entity.Get<RngAttribute>().Seed);

            _world.NewEntity().Get<ChronicleEvent>().Line = line;
            _world.NewEntity().Get<ChangeScreenEvent>().Target = ScreenId.Epilogue;
        }
    }

    private string Summary(int day, BalanceConfig balance, TreasuryAttribute treasury, int defence, bool victory)
    {
        _text.Length = 0;
        _text.Append("Ты продержался ").Append(day).Append(" из ").Append(balance.DaysUntilSiege).AppendLine(" дней.");
        _text.Append("Осталось: ").Append(treasury.Gold).Append(" золота, ")
             .Append(treasury.Food).Append(" пищи, ").Append(treasury.Garrison).AppendLine(" копий гарнизона.");

        if (defence > 0 || victory)
            _text.Append("Стены защищали ").Append(defence).Append(" против ").Append(balance.SiegeStrength).Append('.');

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
using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Стычки под стенами. Каждые две секунды в свободной точке вспыхивает бой:
/// с обеих сторон выходит по отряду, кидается кубик, и через четыре секунды
/// проигравший отряд вычитается из общего числа.
///
/// Кубик — единственное, что решает исход, поэтому малым войском выиграть можно,
/// а большим проиграть. Мораль только двигает шансы: поднимает размер наших отрядов
/// и бросок. Командир добавляет сверху, и один лорд ведёт одну стычку —
/// вот почему важно созвать всех.
///
/// Точками владеют акторы: стычка это BattleAttribute, который на точке появляется
/// и снимается. Никаких массивов индексов в UI.
/// </summary>
public class BattleSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private const int NoPoint = -1;

    private EcsWorld _world;

    private EcsFilter<BattlePinClickedEvent> _clicks;
    private EcsFilter<CloseBattleCardEvent> _closes;

    private EcsFilter<RunFlag, SiegeAttribute> _runs;
    private EcsFilter<BattlePointAttribute, BattleAttribute> _active;
    private EcsFilter<BattlePointAttribute>.Exclude<BattleAttribute> _free;
    private EcsFilter<LordFlag, LordIdAttribute, PersonAttribute, CameToSiegeFlag>.Exclude<DeadFlag> _commanders;

    private int _shown = NoPoint;
    private BattleAttribute _shownBattle;
    private bool _shownFinished;

    public void Init() => Subscribe(true);
    public void Destroy() => Subscribe(false);

    public void Run()
    {
        foreach (var i in _clicks) Open(_clicks.Get1(i).Point);
        foreach (var _ in _closes) Close();

        foreach (var r in _runs)
        {
            ref var siege = ref _runs.Get2(r);

            if (!siege.Running)
            {
                ClearAll();
                continue;
            }

            Resolve(ref siege);
            Spawn(_runs.GetEntity(r), ref siege);
            RepaintPoints();
        }

        RepaintCard();
    }

    // ─────────────────────── рождение стычки ───────────────────────

    private void Spawn(EcsEntity run, ref SiegeAttribute siege)
    {
        var balance = GameConfig.BalanceConfig;
        if (Time.time < siege.NextSpawn) return;

        siege.NextSpawn = Time.time + balance.BattleSpawnInterval;

        if (siege.OurForce <= 0 || siege.EnemyForce <= 0) return;
        if (!TryFreePoint(out var point)) return;

        var rng = run.Get<RngAttribute>().Value;
        if (rng == null) return;

        int squadBonus = siege.Morale / Mathf.Max(1, balance.MoraleToSquad);
        int ourSquad = Mathf.Min(siege.OurForce, rng.Next(balance.SquadMin, balance.SquadMax + 1) + squadBonus);
        int enemySquad = Mathf.Min(siege.EnemyForce, rng.Next(balance.SquadMin, balance.SquadMax + 1));

        int commander = TakeCommander(balance, out int commanderBonus);

        ref var battle = ref point.Get<BattleAttribute>();
        battle.OurSquad = ourSquad;
        battle.EnemySquad = enemySquad;
        battle.CommanderLordId = commander;
        battle.OurRoll = rng.Next(1, 7) + rng.Next(1, 7)
            + siege.Morale / Mathf.Max(1, balance.MoraleToRoll) + commanderBonus;
        battle.EnemyRoll = rng.Next(1, 7) + rng.Next(1, 7) 
            + GameConfig.BalanceConfig.Difficulty / Mathf.Max(1, balance.Difficulty);
        battle.OurWin = battle.OurRoll >= battle.EnemyRoll;
        battle.StartedAt = Time.time;
        battle.EndsAt = Time.time + balance.BattleDuration;
    }

    /// <summary>Игрок ведёт одну стычку, каждый приехавший лорд — тоже одну.
    /// Кому командира не досталось, дерётся без прибавки.</summary>
    private int TakeCommander(BalanceConfig balance, out int bonus)
    {
        if (!PlayerBusy())
        {
            bonus = balance.PlayerCommanderBonus;
            return BattleAttribute.PlayerCommander;
        }

        foreach (var i in _commanders)
        {
            var lord = _commanders.GetEntity(i);
            if (lord.Has<CommandingFlag>()) continue;

            lord.Get<CommandingFlag>();
            bonus = balance.LordCommanderBonus;
            return _commanders.Get2(i).Value;
        }

        bonus = 0;
        return BattleAttribute.NoCommander;
    }

    private bool PlayerBusy()
    {
        foreach (var b in _active)
            if (_active.Get2(b).CommanderLordId == BattleAttribute.PlayerCommander) return true;

        return false;
    }

    // ─────────────────────── исход ───────────────────────

    private void Resolve(ref SiegeAttribute siege)
    {
        foreach (var b in _active)
        {
            ref var battle = ref _active.Get2(b);
            if (Time.time < battle.EndsAt) continue;

            if (battle.OurWin) siege.EnemyForce = Mathf.Max(0, siege.EnemyForce - battle.EnemySquad);
            else siege.OurForce = Mathf.Max(0, siege.OurForce - battle.OurSquad);

            FreeCommander(battle.CommanderLordId);

            var entity = _active.GetEntity(b);
            int point = _active.Get1(b).Index;

            // Карточка не захлопывается на полуслове: если смотрели именно эту стычку,
            // она остаётся открытой с готовым исходом, пока игрок сам не закроет.
            if (_shown == point)
            {
                _shownBattle = battle;
                _shownFinished = true;
            }

            entity.Get<BattleRef>().Value?.Hide();
            entity.Del<BattleAttribute>();
        }
    }

    private void FreeCommander(int lordId)
    {
        if (lordId < 0) return;

        foreach (var i in _commanders)
        {
            if (_commanders.Get2(i).Value != lordId) continue;

            var lord = _commanders.GetEntity(i);
            if (lord.Has<CommandingFlag>()) lord.Del<CommandingFlag>();
            return;
        }
    }

    // ─────────────────────── булавки ───────────────────────

    private void RepaintPoints()
    {
        var balance = GameConfig.BalanceConfig;

        foreach (var b in _active)
        {
            var actor = _active.GetEntity(b).Get<BattleRef>().Value;
            if (actor == null) continue;

            actor.Show(Share(_active.Get2(b), balance));
        }
    }

    /// <summary>Доля наших на полоске: старт ровно посередине, за отведённое время
    /// уползает к исходу.</summary>
    private static float Share(BattleAttribute battle, BalanceConfig balance)
    {
        float progress = Mathf.Clamp01((Time.time - battle.StartedAt) / Mathf.Max(0.01f, balance.BattleDuration));
        return Mathf.Lerp(0.5f, battle.OurWin ? 1f : 0f, progress);
    }

    // ─────────────────────── карточка ───────────────────────

    private void Open(int point)
    {
        if (!TryFindActive(point, out var battle)) return;

        _shown = point;
        _shownBattle = battle;
        _shownFinished = false;
    }

    private void Close()
    {
        _shown = NoPoint;
        _shownFinished = false;
        UI.Battle.SetVisible(false);
    }

    private void RepaintCard()
    {
        if (_shown == NoPoint) return;

        if (!_shownFinished && TryFindActive(_shown, out var battle)) _shownBattle = battle;

        var balance = GameConfig.BalanceConfig;
        string state = _shownFinished
            ? (_shownBattle.OurWin ? "Отряд противника уничтожен" : "Отряд потерян")
            : "Идёт бой";

        UI.Battle.Show(
            CommanderName(_shownBattle.CommanderLordId),
            $"{_shownBattle.OurSquad} против {_shownBattle.EnemySquad}",
            $"Бросок {_shownBattle.OurRoll} — {_shownBattle.EnemyRoll}",
            state,
            _shownFinished ? (_shownBattle.OurWin ? 1f : 0f) : Share(_shownBattle, balance));
    }

    private string CommanderName(int lordId)
    {
        if (lordId == BattleAttribute.PlayerCommander) return "Ведёшь ты";
        if (lordId == BattleAttribute.NoCommander) return "Без командира";

        foreach (var i in _commanders)
            if (_commanders.Get2(i).Value == lordId) return "Ведёт " + _commanders.Get3(i).GivenName;

        return "Без командира";
    }

    // ─────────────────────── мелочи ───────────────────────

    private bool TryFindActive(int point, out BattleAttribute battle)
    {
        foreach (var b in _active)
        {
            if (_active.Get1(b).Index != point) continue;
            battle = _active.Get2(b);
            return true;
        }

        battle = default;
        return false;
    }

    private bool TryFreePoint(out EcsEntity point)
    {
        foreach (var f in _free)
        {
            point = _free.GetEntity(f);
            return true;
        }

        point = default;
        return false;
    }

    private void ClearAll()
    {
        foreach (var b in _active)
        {
            var entity = _active.GetEntity(b);
            entity.Get<BattleRef>().Value?.Hide();
            entity.Del<BattleAttribute>();
        }

        if (_shown != NoPoint) Close();
    }

    private void Subscribe(bool on)
    {
        if (UI.Battle.CloseButton == null) return;

        if (on) UI.Battle.CloseButton.onClick.AddListener(RequestClose);
        else UI.Battle.CloseButton.onClick.RemoveListener(RequestClose);
    }

    private void RequestClose() => _world.NewEntity().Get<CloseBattleCardEvent>();
}
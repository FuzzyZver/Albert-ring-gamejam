using System.Text;
using UnityEngine;
using Leopotam.Ecs;

public class InitSystem : Injects, IEcsInitSystem
{
    private EcsWorld _world;

    public void Init()
    {
        var chars = GameConfig.CharactersConfig;
        var balance = GameConfig.BalanceConfig;

        int seed = System.Environment.TickCount;
        var court = CourtGenerator.Generate(chars, balance, seed);

        CreateRun(court, balance);
        CreatePerson(court.Player, balance);
        foreach (var lord in court.Lords)
            CreatePerson(lord, balance);

        // Этап 1: когда на карте появятся булавки, вместо CreatePerson для лордов —
        // actor.Bind(lord, balance); actor.Init(_world);
        // Компоненты те же, их собирает LordFactory.

        Debug.Log(Describe(court, chars));
    }

    private void CreateRun(CourtData court, BalanceConfig balance)
    {
        var entity = _world.NewEntity();
        entity.Get<RunFlag>();

        ref var calendar = ref entity.Get<CalendarAttribute>();
        calendar.Day = 1;
        calendar.Phase = DayPhase.Morning;
        calendar.ActionsLeft = balance.ActionsPerDay;

        ref var treasury = ref entity.Get<TreasuryAttribute>();
        treasury.Gold = balance.StartGold;
        treasury.Food = balance.StartFood;
        treasury.Garrison = balance.StartGarrison;

        ref var tax = ref entity.Get<TaxAttribute>();
        tax.Peasants = 1;
        tax.Lords = 1;

        entity.Get<CommonsAttribute>().Opinion = balance.StartCommonsOpinion;

        ref var rng = ref entity.Get<RngAttribute>();
        rng.Seed = court.Seed;
        rng.Value = new System.Random(court.Seed);

        entity.Get<RunStartEvent>().Seed = court.Seed;
    }

    private void CreatePerson(LordData data, BalanceConfig balance)
    {
        LordFactory.Fill(_world.NewEntity(), data, balance);
    }

    // ──────────── проверка этапа: двор целиком в консоли ────────────

    private static string Describe(CourtData court, CharactersConfig chars)
    {
        var text = new StringBuilder();
        text.AppendLine($"═══ Двор, сид {court.Seed} ═══");
        text.AppendLine($"Ты: {court.Player.FullName} · {TraitLine(court.Player, chars)}");

        foreach (var lord in court.Lords)
        {
            var ambition = chars.GetAmbition(lord.Ambition);
            var rival = court.Lords.Find(l => l.Id == lord.RivalId);

            text.Append($"{lord.Id}. {lord.FullName} — {TraitLine(lord, chars)} · {lord.Troops} копий");
            if (ambition != null) text.Append($" · хочет: {ambition.Title.ToLower()}");
            text.Append(rival != null ? $" · враг: {rival.FullName}" : " · врагов нет");
            text.AppendLine();
        }

        return text.ToString();
    }

    private static string TraitLine(LordData data, CharactersConfig chars)
    {
        var a = chars.GetTrait(data.TraitA);
        var b = chars.GetTrait(data.TraitB);
        return $"{Name(a, data.Gender)}, {Name(b, data.Gender)}";
    }

    private static string Name(TraitDefinition trait, Gender gender) =>
        trait != null ? trait.GetTitle(gender) : "???";
}
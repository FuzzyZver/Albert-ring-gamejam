using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Единственное место, где рождается забег. И старт, и «новый забег» идут сюда,
/// поэтому рестарт не может разойтись со стартом.
/// </summary>
public class RunSetupSystem : Injects, IEcsRunSystem
{
    private EcsFilter<NewRunEvent> _requests;
    private EcsFilter<RunFlag> _runs;
    private EcsFilter<PersonAttribute> _persons;   // лорды и игрок

    public void Run()
    {
        foreach (var i in _requests)
        {
            int seed = _requests.Get1(i).Seed;
            Clear();
            Build(seed);
        }
    }

    private void Clear()
    {
        foreach (var i in _runs) _runs.GetEntity(i).Destroy();
        foreach (var i in _persons) _persons.GetEntity(i).Destroy();
    }

    private void Build(int seed)
    {
        var court = CourtGenerator.Generate(
            GameConfig.CharactersConfig, GameConfig.BalanceConfig, seed);

        CreateRun(court);
        SpawnLords(court);

        EcsWorld.NewEntity().Get<CourtReadyEvent>();
    }

    private void CreateRun(CourtData court)
    {
        var balance = GameConfig.BalanceConfig;
        var entity = EcsWorld.NewEntity();
        entity.Get<RunFlag>();
        entity.Get<CourtAttribute>().Value = court;

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
    }

    private void SpawnLords(CourtData court)
    {
        var pins = SceneData.LordPins;
        if (pins == null || pins.Length == 0)
        {
            Debug.LogError("SceneData.LordPins пуст — лордов некуда сажать");
            return;
        }

        if (pins.Length < court.Lords.Count)
            Debug.LogWarning($"Булавок {pins.Length}, лордов {court.Lords.Count} — часть двора не попадёт на карту");

        for (int i = 0; i < court.Lords.Count && i < pins.Length; i++)
        {
            pins[i].Bind(court.Lords[i], GameConfig.BalanceConfig);
            pins[i].Init(EcsWorld);
        }
    }
}
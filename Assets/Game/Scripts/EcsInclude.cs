using System.Collections.Generic;
using UnityEngine;
using Leopotam.Ecs;

public class EcsInclude : MonoBehaviour
{
    [SerializeField] private UI _ui;
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private SceneData _sceneData;
    private EcsWorld _world;
    private EcsSystems _systems;

    public void Awake()
    {
        _world = new EcsWorld();
        _systems = new EcsSystems(_world);

        _world = new EcsWorld();
        _systems = new EcsSystems(_world);

        _systems
            //Add (new ...
            .Add(new InitSystem())
            .Add(new RunSetupSystem())
            .Add(new PlayerSpawnSystem())
            .Add(new CandidateScreenSystem())   // после PlayerSpawn: ловит RunReadyEvent

            .Add(new PhaseSystem())
            .Add(new NightSystem())
            .Add(new DeathWatchSystem())    // сразу после ночного счёта, до последствий
            .Add(new SiegeSystem())         // после смертей: голод важнее армии у ворот
            .Add(new CastleActionSystem())

            .Add(new SelectionSystem())
            .Add(new VerbActionSystem())
            .Add(new ConsequenceSystem())   // ловит и глаголы, и ночные риски черт
            .Add(new DuelSystem())
            .Add(new OpinionSystem())
            .Add(new VerbResolveSystem())   // строго после мнений: считает уже по новым цифрам
            .Add(new RunEndSystem())        // до ScreenSystem: эпилог просит экран событием

            .Add(new ScreenSystem())
            .Add(new MapViewSystem())
            .Add(new HudSystem())
            .Add(new EveningSystem())
            .Add(new LordCardSystem())
            .Add(new VerbPanelSystem())
            .Add(new ChronicleSystem())
            .Add(new RestartSystem())

            //OneFrame<..
            .OneFrame<NewRunEvent>()
            .OneFrame<CourtReadyEvent>()
            .OneFrame<SelectCandidateEvent>()
            .OneFrame<RunReadyEvent>()

            .OneFrame<AdvancePhaseEvent>()
            .OneFrame<PhaseEndedEvent>()
            .OneFrame<PhaseChangedEvent>()
            .OneFrame<DayStartedEvent>()
            .OneFrame<SpendActionEvent>()

            .OneFrame<ChangeScreenEvent>()
            .OneFrame<CastleSlotClickedEvent>()
            .OneFrame<SetTaxEvent>()

            .OneFrame<VerbEvent>()
            .OneFrame<VerbResolvedEvent>()
            .OneFrame<ConsequenceEvent>()
            .OneFrame<OpinionChangeEvent>()
            .OneFrame<CourtOpinionChangeEvent>()
            .OneFrame<CommonsOpinionChangeEvent>()
            .OneFrame<ChronicleEvent>()

            .OneFrame<EveningChoiceEvent>()
            .OneFrame<DeathEvent>()
            .OneFrame<VictoryEvent>()
            .OneFrame<DuelResolvedEvent>()

            .OneFrame<PinClickedEvent>()
            .OneFrame<CloseCardEvent>()
            .OneFrame<SelectionChangedFlag>()   // снимается здесь, когда вьюхи уже перерисовались



            .Inject(_world)
            .Inject(_gameConfig)
            .Inject(_ui)
            .Inject(_sceneData)


            .Init();
    }

    public void Update()
    {
        _systems.Run();
    }
    private void OnDestroy()
    {
        _systems?.Destroy();
        _systems = null;
        _world?.Destroy();
        _world = null;
    }
}
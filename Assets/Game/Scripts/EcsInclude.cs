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

        _systems
            //Add (new ...
            // Порядок здесь — не вкусовщина. Одно-кадровые события живут до конца кадра,
            // поэтому система, которая событие СОЗДАЁТ, обязана стоять раньше той,
            // которая его читает. Иначе оно молча исчезнет, не дожив до следующего кадра.
            // Исключение — события из колбэков кнопок: они рождаются между кадрами.
            .Add(new InitSystem())
            .Add(new RunSetupSystem())
            .Add(new PlayerSpawnSystem())
            .Add(new MetaTextSystem())
            .Add(new CandidateScreenSystem())   // после PlayerSpawn: ловит RunReadyEvent

            .Add(new PhaseSystem())
            .Add(new NightSystem())
            .Add(new DeathWatchSystem())    // сразу после ночного счёта, до последствий
            .Add(new SiegeSystem())
            .Add(new BattleSystem())         // после смертей: голод важнее армии у ворот
            .Add(new BuildingSystem())
            .Add(new CastleActionSystem())

            .Add(new SelectionSystem())
            .Add(new VerbActionSystem())
            .Add(new PetitionSystem())      // утро у трона
            .Add(new EveningSystem())       // очередь вечера
            .Add(new ChoiceEffectSystem())  // применяет выбор просителя или события
            .Add(new DuelSystem())          // после ChoiceEffect: выбор тоже может вызвать на поединок
            .Add(new ConsequenceSystem())   // ловит и глаголы, и ночные риски черт
            .Add(new OpinionSystem())
            .Add(new VerbResolveSystem())   // строго после мнений: считает уже по новым цифрам
            .Add(new RunEndSystem())        // до ScreenSystem: эпилог просит экран событием

            .Add(new ScreenSystem())
            .Add(new MapViewSystem())
            .Add(new HudSystem())
            .Add(new EveningViewSystem())   // после DuelSystem: тело поединка дописано
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

            .OneFrame<ChangeScreenEvent>()
            .OneFrame<BuildingPinClickedEvent>()
            .OneFrame<BuildRequestEvent>()
            .OneFrame<CastleActionsPinClickedEvent>()
            .OneFrame<CastleActionRequestEvent>()
            .OneFrame<CloseCastleCardEvent>()
            .OneFrame<SetTaxEvent>()

            .OneFrame<VerbEvent>()
            .OneFrame<ConsequenceEvent>()
            .OneFrame<OpinionChangeEvent>()
            .OneFrame<CourtOpinionChangeEvent>()
            .OneFrame<CommonsOpinionChangeEvent>()
            .OneFrame<ChronicleEvent>()

            .OneFrame<EveningChoiceEvent>()
            .OneFrame<PetitionChoiceEvent>()
            .OneFrame<CallNextPetitionerEvent>()
            .OneFrame<QueueEveningEvent>()
            .OneFrame<ApplyChoiceEvent>()
            .OneFrame<DeathEvent>()
            .OneFrame<VictoryEvent>()
            .OneFrame<SiegeStartedEvent>()
            .OneFrame<BattlePinClickedEvent>()
            .OneFrame<CloseBattleCardEvent>()

            .OneFrame<PinClickedEvent>()
            .OneFrame<CloseCardEvent>()
            .OneFrame<SelectionChangedFlag>()   // снимается здесь, когда вьюхи уже перерисовались
            .OneFrame<MetaTextEnterEvent>()
            .OneFrame<MetaTextExitEvent>()

            .Inject(_world)
            .Inject(_gameConfig)
            .Inject(_ui)
            .Inject(_sceneData)
            .Inject(new RealtimeData())


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
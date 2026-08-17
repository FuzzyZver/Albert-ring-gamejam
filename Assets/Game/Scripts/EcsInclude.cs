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
            .Add(new InitSystem())
            .Add(new RunSetupSystem())
            .Add(new CandidateScreenSystem())
            .Add(new PlayerSpawnSystem())
            .Add(new MapViewSystem())
            .Add(new LordCardSystem())
            .Add(new RestartSystem())

            //OneFrame<..
            .OneFrame<NewRunEvent>()
            .OneFrame<CourtReadyEvent>()
            .OneFrame<SelectCandidateEvent>()
            .OneFrame<RunReadyEvent>()
            .OneFrame<PinClickedEvent>()
            .OneFrame<CloseCardEvent>()



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
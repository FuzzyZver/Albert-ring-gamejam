using Leopotam.Ecs;

/// <summary>Кнопка «новый забег». Ничего не сносит сама — просто просит новый сид,
/// а всю уборку делает RunSetupSystem.</summary>
public class RestartSystem : Injects, IEcsInitSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    public void Init()
    {
        if (UI.NewRunButton != null)
            UI.NewRunButton.onClick.AddListener(RequestNewRun);
        if (UI.Epilogue != null && UI.Epilogue.RestartButton != null)
            UI.Epilogue.RestartButton.onClick.AddListener(RequestNewRun);
    }

    public void Destroy()
    {
        if (UI.NewRunButton != null)
            UI.NewRunButton.onClick.RemoveListener(RequestNewRun);
        if (UI.Epilogue != null && UI.Epilogue.RestartButton != null)
            UI.Epilogue.RestartButton.onClick.RemoveListener(RequestNewRun);
    }

    private void RequestNewRun()
    {
        _world.NewEntity().Get<NewRunEvent>().Seed = GameConfig.BalanceConfig.NextSeed();
    }
}
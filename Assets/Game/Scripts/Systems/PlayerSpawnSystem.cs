using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Выбранный кандидат становится игроком. Сущность создаёт булавка замка,
/// а не система: тогда клик по замку работает ровно так же, как клик по лорду,
/// и кода на это не нужно вообще.
/// </summary>
public class PlayerSpawnSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<SelectCandidateEvent> _requests;
    private EcsFilter<RunFlag, CourtAttribute> _runs;

    public void Run()
    {
        foreach (var i in _requests)
        {
            int index = _requests.Get1(i).Index;

            foreach (var r in _runs)
            {
                var court = _runs.Get2(r).Value;
                if (index < 0 || index >= court.Candidates.Count) continue;
                if (court.Player != null) continue;   // уже выбрали, второй клик игнорируем

                court.Player = court.Candidates[index];

                var castle = SceneData.PlayerCastle;
                if (castle == null)
                {
                    Debug.LogError("SceneData.PlayerCastle не назначен — игроку негде жить");
                    continue;
                }

                castle.Bind(court.Player, GameConfig.BalanceConfig);
                castle.Init(_world);

                _world.NewEntity().Get<RunReadyEvent>();
                UI.CharacterSelect.SetVisible(false);
            }
        }
    }
}
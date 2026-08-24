using Leopotam.Ecs;

/// <summary>
/// Единственное место, где двигается время. Шлёт два события вместо одного:
/// PhaseEnded для тех, кому надо закрыть фазу (ночной счёт), и PhaseChanged
/// для тех, кому надо открыть новую. Так порядок систем в конвейере
/// перестаёт влиять на результат.
/// </summary>
public class PhaseSystem : Injects, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsFilter<AdvancePhaseEvent> _requests;
    private EcsFilter<RunFlag, CalendarAttribute>.Exclude<RunOverFlag, PhaseLockFlag> _runs;

    public void Run()
    {
        bool moved = false;

        foreach (var _ in _requests)
        {
            if (moved) break;   // два клика в один кадр не должны съедать две фазы
            moved = Advance();
        }
    }

    private bool Advance()
    {
        foreach (var r in _runs)
        {
            ref var calendar = ref _runs.Get2(r);

            ref var ended = ref _world.NewEntity().Get<PhaseEndedEvent>();
            ended.Phase = calendar.Phase;
            ended.Day = calendar.Day;

            if (calendar.Phase == DayPhase.Night)
            {
                calendar.Day++;
                calendar.Phase = DayPhase.Morning;
                calendar.ActionsLeft = GameConfig.BalanceConfig.ActionsPerDay;
            }
            else
            {
                calendar.Phase = (DayPhase)((int)calendar.Phase + 1);
            }

            _world.NewEntity().Get<PhaseChangedEvent>().Phase = calendar.Phase;
            return true;
        }

        return false;
    }
}
using Leopotam.Ecs;

/// <summary>Складывает строки летописи в окно. Отдельной системой, чтобы любая
/// другая могла просто бросить ChronicleEvent и не знать про UI.</summary>
public class ChronicleSystem : Injects, IEcsRunSystem
{
    private EcsFilter<ChronicleEvent> _lines;
    private EcsFilter<CourtReadyEvent> _newRuns;

    public void Run()
    {
        foreach (var _ in _newRuns) UI.Chronicle.Clear();
        foreach (var i in _lines) UI.Chronicle.Append(_lines.Get1(i).Line);
    }
}
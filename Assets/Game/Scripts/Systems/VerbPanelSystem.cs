using Leopotam.Ecs;

/// <summary>
/// Панель глаголов. Ничего не считает и ни на что не подписывается кроме кнопок:
/// строки приходят готовыми из VerbOffersAttribute.
/// </summary>
public class VerbPanelSystem : Injects, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private EcsWorld _world;

    private EcsFilter<RunFlag, SelectionAttribute, VerbOffersAttribute> _runs;
    private EcsFilter<PlayerFlag, PersonAttribute, TraitsAttribute> _players;

    public void Init() => Subscribe(true);
    public void Destroy() => Subscribe(false);

    public void Run()
    {
        foreach (var r in _runs)
        {
            if (!_runs.GetEntity(r).Has<SelectionChangedFlag>()) continue;
            Repaint(r);
        }
    }

    private void Repaint(int runIndex)
    {
        ref var selection = ref _runs.Get2(runIndex);
        var offers = _runs.Get3(runIndex).Value;

        if (!selection.HasTarget || selection.IsPlayer || offers == null || offers.Count == 0)
        {
            UI.VerbPanel.SetVisible(false);
            return;
        }

        UI.VerbPanel.SetVisible(true);
        UI.VerbPanel.SetPlayerLine(PlayerLine());
        UI.VerbPanel.HideAll();

        var rows = UI.VerbPanel.Rows;
        if (rows == null) return;

        for (int i = 0; i < offers.Count && i < rows.Length; i++)
            rows[i].Set(offers[i]);
    }

    private string PlayerLine()
    {
        var chars = GameConfig.CharactersConfig;

        foreach (var p in _players)
        {
            ref var person = ref _players.Get2(p);
            ref var traits = ref _players.Get3(p);
            return $"Ты: {person.FullName} · {chars.TraitLine(traits.A, traits.B, person.Gender)}";
        }

        return string.Empty;
    }

    private void Subscribe(bool on)
    {
        var rows = UI.VerbPanel.Rows;
        if (rows == null) return;

        for (int i = 0; i < rows.Length; i++)
        {
            var row = rows[i];
            if (row == null || row.Button == null) continue;

            if (on) row.Button.onClick.AddListener(() => Use(row));
            else row.Button.onClick.RemoveAllListeners();
        }
    }

    private void Use(VerbRowView row)
    {
        foreach (var r in _runs)
        {
            ref var selection = ref _runs.Get2(r);
            if (!selection.HasTarget || selection.IsPlayer) return;

            ref var request = ref _world.NewEntity().Get<VerbEvent>();
            request.TargetLordId = selection.LordId;
            request.Verb = row.Verb;
        }
    }
}
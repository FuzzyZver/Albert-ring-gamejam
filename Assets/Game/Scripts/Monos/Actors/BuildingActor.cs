using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Leopotam.Ecs;

/// <summary>
/// Булавка постройки в замке. В отличие от булавки действий у неё есть настоящее
/// состояние — уровень и стройка, — поэтому она владеет сущностью.
/// Порядок вызова обычный: Init(world) из RunSetupSystem.
/// </summary>
public class BuildingActor : Actor
{
    [SerializeField] private BuildingId _id;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private TMP_Text _level;

    public BuildingId Id => _id;

    public override void ExpandEntity(EcsEntity entity)
    {
        ref var building = ref entity.Get<BuildingAttribute>();
        building.Id = _id;
        building.Level = 0;
        building.TargetLevel = 0;

        entity.Get<BuildingRef>().Value = this;
        entity.Get<TransformRef>().Value = transform;
    }

    public void SetLabel(string title, string level)
    {
        if (_label != null) _label.text = title;
        if (_level != null) _level.text = level;
    }

    private void Awake()
    {
        if (_button != null) _button.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        if (_button != null) _button.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        var world = GetWorld();
        if (world == null) return;

        world.NewEntity().Get<BuildingPinClickedEvent>().Id = _id;
    }
}
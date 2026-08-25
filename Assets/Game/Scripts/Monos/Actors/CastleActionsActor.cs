using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Leopotam.Ecs;

/// <summary>
/// Булавка действий: пир, служба, вербовка. Своего состояния не держит —
/// что и когда устраивали, помнит забег в CastleHistoryAttribute.
/// Сущность заводится ради привязки к сцене, как у любого актора.
/// </summary>
public class CastleActionsActor : Actor
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _label;

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<CastleActionsFlag>();
        entity.Get<TransformRef>().Value = transform;
    }

    public void SetLabel(string text)
    {
        if (_label != null) _label.text = text;
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

        world.NewEntity().Get<CastleActionsPinClickedEvent>();
    }
}
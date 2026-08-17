using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Leopotam.Ecs;

/// <summary>
/// Булавка на карте. Порядок вызова: Bind(...) -> Init(world).
/// Используется и для лордов, и для замка игрока — разницу делает LordData.Id.
///
/// Клик актор шлёт в мир сам: он единственный, кто знает свою сущность.
/// Ради этого в базовом Actor и лежат GetWorld() и GetEntity().
/// </summary>
public class LordActor : Actor
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Transform _pin;

    private LordData _data;
    private BalanceConfig _balance;

    public void Bind(LordData data, BalanceConfig balance)
    {
        _data = data;
        _balance = balance;
        SetLabel(data != null ? data.FullName : string.Empty);
    }

    public override void ExpandEntity(EcsEntity entity)
    {
        if (_data == null)
        {
            Debug.LogError($"{name}: Init вызван до Bind, булавка пустая", this);
            return;
        }

        LordFactory.Fill(entity, _data, _balance);

        entity.Get<ActorRef>().Value = this;
        entity.Get<TransformRef>().Value = transform;
        entity.Get<PinRef>().Value = _pin != null ? _pin : transform;
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

        var entity = GetEntity();
        if (!entity.IsAlive()) return;
        world.NewEntity().Get<PinClickedEvent>().Target = entity;
    }
}
using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Булавка лорда на карте. Порядок вызова: Bind(...) -> Init(world).
/// Bind обязателен, иначе ExpandEntity нечем наполнять.
/// </summary>
public class LordActor : Actor
{
    [SerializeField] private Transform _pin;

    private LordData _data;
    private BalanceConfig _balance;

    public void Bind(LordData data, BalanceConfig balance)
    {
        _data = data;
        _balance = balance;
    }

    public override void ExpandEntity(EcsEntity entity)
    {
        if (_data == null)
        {
            Debug.LogError($"{name}: Init вызван до Bind, лорд пустой", this);
            return;
        }

        LordFactory.Fill(entity, _data, _balance);

        entity.Get<ActorRef>().Value = this;
        entity.Get<TransformRef>().Value = transform;
        entity.Get<PinRef>().Value = _pin != null ? _pin : transform;
    }
}
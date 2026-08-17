using System.Collections.Generic;
using Leopotam.Ecs;

/// <summary>
/// Собирает компоненты лорда из LordData. Статика здесь — только чистые функции
/// без единого поля, состояния он не держит. Нужен в двух местах: InitSystem
/// (когда лорд без булавки на карте) и LordActor.ExpandEntity (когда с булавкой),
/// поэтому вынесен, чтобы не дублировать список компонентов.
/// Если статика принципиально не нравится — сделай его обычным классом
/// и положи поле в Injects, интерфейс не изменится.
/// </summary>
public static class LordFactory
{
    public static void Fill(EcsEntity entity, LordData data, BalanceConfig balance)
    {
        entity.Get<LordIdAttribute>().Value = data.Id;

        ref var person = ref entity.Get<PersonAttribute>();
        person.Title = data.Title;
        person.GivenName = data.GivenName;
        person.Epithet = data.Epithet;
        person.Gender = data.Gender;

        ref var traits = ref entity.Get<TraitsAttribute>();
        traits.A = data.TraitA;
        traits.B = data.TraitB;

        if (data.Id < 0)
        {
            entity.Get<PlayerFlag>();
            return;
        }

        entity.Get<LordFlag>();
        entity.Get<AtCourtFlag>();
        entity.Get<OpinionAttribute>().Value = balance.StartLordOpinion;
        entity.Get<TroopsAttribute>().Value = data.Troops;
        entity.Get<AmbitionAttribute>().Id = data.Ambition;
        entity.Get<RivalAttribute>().LordId = data.RivalId;
        entity.Get<SpentVerbsAttribute>().Value = new List<VerbId>();
    }
}
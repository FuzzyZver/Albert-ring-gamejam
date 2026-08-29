using UnityEngine.EventSystems;

/// <summary>
/// Вешается на любую вьюху, которой есть что сказать при наведении.
/// Попадание считает EventSystem — луч руками пускать не надо.
/// В OnPointerEnter шлём MetaTextEnterEvent, в OnPointerExit — MetaTextExitEvent.
/// </summary>
public interface IMetaText : IPointerEnterHandler, IPointerExitHandler
{
    MetaText Meta { get; }
}
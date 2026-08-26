using UnityEngine;
using UnityEngine.UI;
using Leopotam.Ecs;

/// <summary>
/// Точка боя вокруг замка. Тот же паттерн, что у булавки лорда и булавки постройки:
/// актор владеет сущностью, система кладёт на неё данные, вьюха рисует подробности.
/// Благодаря этому точки можно перенести с холста на физическую сцену — актор
/// уже привязан к своему Transform, менять придётся только префаб.
///
/// Порядок вызова: Bind(index) -> Init(world) из RunSetupSystem.
/// Сущность живёт всю осаду, а стычка — это BattleAttribute, который на ней
/// появляется и снимается.
/// </summary>
public class BattleActor : Actor
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _button;
    [SerializeField] private Image _bar;

    private int _index = -1;

    public void Bind(int index) => _index = index;

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<BattlePointAttribute>().Index = _index;
        entity.Get<BattleRef>().Value = this;
        entity.Get<TransformRef>().Value = transform;

        Hide();
    }

    /// <summary>Полоска над кнопкой: доля наших. Видно издалека, кто где давит.</summary>
    public void Show(float ourShare)
    {
        if (_root != null) _root.SetActive(true);
        if (_bar != null) _bar.fillAmount = ourShare;
    }

    public void Hide()
    {
        if (_root != null) _root.SetActive(false);
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

        world.NewEntity().Get<BattlePinClickedEvent>().Point = _index;
    }
}
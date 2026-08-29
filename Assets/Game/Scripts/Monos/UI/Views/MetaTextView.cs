using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MetaTextView : MonoBehaviour
{
    [SerializeField] private RectTransform _window;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _body;
    [SerializeField] private Vector2 _offset = new Vector2(16f, -16f);

    private Canvas _canvas;
    private RectTransform _canvasRect;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _canvasRect = _canvas != null ? (RectTransform)_canvas.transform : (RectTransform)transform;
    }

    public void SetText(MetaText text)
    {
        bool hasTitle = !string.IsNullOrEmpty(text.Title);
        if (_title != null)
        {
            _title.gameObject.SetActive(hasTitle);
            if (hasTitle) _title.text = text.Title;
        }
        if (_body != null) _body.text = text.Body;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_window);
    }

    public void Place(Vector2 screen)
    {
        if (_canvasRect == null) return;

        var camera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? _canvas.worldCamera
            : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, camera, out var local);

        var size = _window.rect.size;
        var half = _canvasRect.rect.size * 0.5f;
        var position = local + _offset;

        position.x = Mathf.Clamp(position.x, -half.x, half.x - size.x);
        position.y = Mathf.Clamp(position.y, -half.y + size.y, half.y);

        _window.anchoredPosition = position;
    }

    public void Show() { if (!_window.gameObject.activeSelf) _window.gameObject.SetActive(true); }
    public void Hide() { if (_window.gameObject.activeSelf) _window.gameObject.SetActive(false); }
}

using TMPro;
using UnityEngine;

/// <summary>Список глаголов под шапкой карточки. Строк в сцене должно быть
/// не меньше, чем глаголов в конфиге — лишние прячутся сами.</summary>
public class VerbPanelView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _playerLine;
    [SerializeField] private VerbRowView[] _rows;

    public VerbRowView[] Rows => _rows;

    public void SetVisible(bool value)
    {
        if (_root != null) _root.SetActive(value);
    }

    public void SetPlayerLine(string text)
    {
        if (_playerLine != null) _playerLine.text = text;
    }

    public void HideAll()
    {
        if (_rows == null) return;
        for (int i = 0; i < _rows.Length; i++)
            if (_rows[i] != null) _rows[i].Hide();
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Экран летописца. Читается один раз, поэтому всё видно сразу, без вкладок.</summary>
public class EpilogueView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _restartButton;

    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _chronicle;
    [SerializeField] private TMP_Text _summary;
    [SerializeField] private TMP_Text _court;
    [SerializeField] private TMP_Text _seed;

    [Header("Цвета заголовка")]
    [SerializeField] private Color _defeat = new Color(0.90f, 0.35f, 0.35f);
    [SerializeField] private Color _victory = new Color(0.36f, 0.80f, 0.45f);

    public Button RestartButton => _restartButton;

    public void SetVisible(bool value)
    {
        if (_root != null) _root.SetActive(value);
    }

    /// <summary>Только заполняет. Показывает и прячет ScreenSystem — иначе окно
    /// оказывается с двумя хозяевами и однажды не закрывается.</summary>
    public void Show(bool victory, string title, string chronicle, string summary, string court, int seed)
    {
        if (_title != null)
        {
            _title.text = title;
            _title.color = victory ? _victory : _defeat;
        }

        if (_chronicle != null) _chronicle.text = chronicle;
        if (_summary != null) _summary.text = summary;
        if (_court != null) _court.text = court;
        if (_seed != null) _seed.text = "сид " + seed;
    }
}
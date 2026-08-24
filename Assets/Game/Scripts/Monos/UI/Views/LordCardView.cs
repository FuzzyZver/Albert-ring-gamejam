using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Шапка карточки персонажа. Список глаголов живёт отдельно в VerbPanelView,
/// но лежит внутри этого же корня — гасится вместе с карточкой.</summary>
public class LordCardView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _closeButton;

    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _traits;

    [SerializeField] private GameObject _lordOnly;
    [SerializeField] private TMP_Text _ambition;
    [SerializeField] private TMP_Text _ambitionQuote;   // как он это просит, своими словами
    [SerializeField] private TMP_Text _rival;
    [SerializeField] private TMP_Text _opinion;
    [SerializeField] private TMP_Text _willCome;

    [Header("Цвета мнения")]
    [SerializeField] private Color _positive = new Color(0.36f, 0.80f, 0.45f);
    [SerializeField] private Color _negative = new Color(0.90f, 0.35f, 0.35f);

    public Button CloseButton => _closeButton;

    public void SetVisible(bool value)
    {
        if (_root != null) _root.SetActive(value);
    }

    public void ShowPlayer(string fullName, string traits)
    {
        SetVisible(true);
        if (_lordOnly != null) _lordOnly.SetActive(false);
        if (_name != null) _name.text = fullName;
        if (_traits != null) _traits.text = traits;
    }

    public void ShowLord(string fullName, string traits, string ambition, string quote, string rival,
        int opinion, int troops, bool willCome)
    {
        SetVisible(true);
        if (_lordOnly != null) _lordOnly.SetActive(true);

        if (_name != null) _name.text = fullName;
        if (_traits != null) _traits.text = $"{traits} · {troops} копий";
        if (_ambition != null) _ambition.text = "Хочет: " + ambition;

        if (_ambitionQuote != null)
        {
            _ambitionQuote.text = string.IsNullOrEmpty(quote) ? string.Empty : "«" + quote + "»";
            _ambitionQuote.gameObject.SetActive(!string.IsNullOrEmpty(quote));
        }
        if (_rival != null) _rival.text = rival;

        if (_opinion != null)
        {
            _opinion.text = opinion > 0 ? "+" + opinion : opinion.ToString();
            _opinion.color = opinion >= 0 ? _positive : _negative;
        }

        if (_willCome != null)
        {
            _willCome.text = willCome ? "придёт" : "не придёт";
            _willCome.color = willCome ? _positive : _negative;
        }
    }
}
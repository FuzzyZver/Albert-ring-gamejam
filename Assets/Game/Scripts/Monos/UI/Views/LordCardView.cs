using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Карточка персонажа, всплывающая по клику на булавку.
/// Блок _lordOnly прячется для игрока: у тебя нет ни амбиции, ни копий, ни мнения о себе.</summary>
public class LordCardView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _closeButton;

    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _traits;

    [SerializeField] private GameObject _lordOnly;
    [SerializeField] private TMP_Text _ambition;
    [SerializeField] private TMP_Text _rival;
    [SerializeField] private TMP_Text _opinion;
    [SerializeField] private TMP_Text _troops;

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

    public void ShowLord(string fullName, string traits, string ambition, string rival, int opinion, int troops)
    {
        SetVisible(true);
        if (_lordOnly != null) _lordOnly.SetActive(true);
        if (_name != null) _name.text = fullName;
        if (_traits != null) _traits.text = traits;
        if (_ambition != null) _ambition.text = ambition;
        if (_rival != null) _rival.text = rival;
        if (_opinion != null) _opinion.text = opinion > 0 ? "+" + opinion : opinion.ToString();
        if (_troops != null) _troops.text = troops + " копий";
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Одна карточка кандидата в окне выбора. Ничего не решает, только показывает.
/// Если у тебя не TMP, а обычный Text — поменяй тип полей, остальное не изменится.</summary>
public class CandidateCardView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _traits;
    [SerializeField] private TMP_Text _hints;

    public Button Button => _button;

    public void Set(string fullName, string traits, string hints)
    {
        gameObject.SetActive(true);
        if (_name != null) _name.text = fullName;
        if (_traits != null) _traits.text = traits;
        if (_hints != null) _hints.text = hints;
    }

    public void Hide() => gameObject.SetActive(false);
}
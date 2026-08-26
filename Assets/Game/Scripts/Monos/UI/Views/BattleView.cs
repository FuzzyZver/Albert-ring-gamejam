using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Карточка стычки: кто ведёт, сколько кого, что выпало на кубиках.
/// _bar — Image типа Filled, заполнение показывает долю наших.</summary>
public class BattleView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _close;

    [SerializeField] private TMP_Text _commander;
    [SerializeField] private TMP_Text _forces;
    [SerializeField] private TMP_Text _roll;
    [SerializeField] private TMP_Text _state;
    [SerializeField] private Image _bar;

    public Button CloseButton => _close;

    public void SetVisible(bool value)
    {
        if (_root != null) _root.SetActive(value);
    }

    public void Show(string commander, string forces, string roll, string state, float ourShare)
    {
        SetVisible(true);

        if (_commander != null) _commander.text = commander;
        if (_forces != null) _forces.text = forces;
        if (_roll != null) _roll.text = roll;
        if (_state != null) _state.text = state;
        if (_bar != null) _bar.fillAmount = ourShare;
    }
}
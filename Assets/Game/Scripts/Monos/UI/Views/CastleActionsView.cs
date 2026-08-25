using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Карточка булавки действий: что можно устроить сегодня.</summary>
public class CastleActionsView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _close;

    [SerializeField] private Button[] _actions;
    [SerializeField] private TMP_Text[] _titles;
    [SerializeField] private TMP_Text[] _descriptions;
    [SerializeField] private TMP_Text[] _states;

    public Button CloseButton => _close;
    public Button[] Actions => _actions;

    public void SetVisible(bool value)
    {
        if (_root != null) _root.SetActive(value);
    }

    public void SetAction(int index, string title, string description, string state, bool available)
    {
        if (_actions == null || index < 0 || index >= _actions.Length) return;

        if (_actions[index] != null)
        {
            _actions[index].gameObject.SetActive(true);
            _actions[index].interactable = available;
        }

        if (_titles != null && index < _titles.Length && _titles[index] != null)
            _titles[index].text = title;

        if (_descriptions != null && index < _descriptions.Length && _descriptions[index] != null)
            _descriptions[index].text = description;

        if (_states != null && index < _states.Length && _states[index] != null)
            _states[index].text = state;
    }

    public void HideFrom(int index)
    {
        if (_actions == null) return;

        for (int i = index; i < _actions.Length; i++)
            if (_actions[i] != null) _actions[i].gameObject.SetActive(false);
    }
}
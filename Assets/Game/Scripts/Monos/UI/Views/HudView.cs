using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Верхняя полоса: день, время суток, ресурсы; сбоку — оставшиеся действия.
/// _phaseMarks — по одному объекту на фазу в порядке DayPhase, активен текущий.</summary>
public class HudView : MonoBehaviour
{
    [SerializeField] private GameObject _root;

    [Header("Время")]
    [SerializeField] private TMP_Text _day;
    [SerializeField] private TMP_Text _phase;
    [SerializeField] private GameObject[] _phaseMarks;

    [Header("Ресурсы")]
    [SerializeField] private TMP_Text _gold;
    [SerializeField] private TMP_Text _food;
    [SerializeField] private TMP_Text _garrison;
    [SerializeField] private TMP_Text _actions;

    [Header("Кнопки")]
    [SerializeField] private Button _nextPhase;
    [SerializeField] private TMP_Text _nextPhaseLabel;
    [SerializeField] private Button _map;
    [SerializeField] private Button _court;
    [SerializeField] private Button _castle;

    public Button NextPhaseButton => _nextPhase;
    public Button MapButton => _map;
    public Button CourtButton => _court;
    public Button CastleButton => _castle;

    public void SetVisible(bool value)
    {
        if (_root != null) _root.SetActive(value);
    }

    public void SetDay(int day, int total)
    {
        if (_day != null) _day.text = $"День {day} из {total}";
    }

    public void SetPhase(DayPhase phase, string title, string buttonLabel)
    {
        if (_phase != null) _phase.text = title;
        if (_nextPhaseLabel != null) _nextPhaseLabel.text = buttonLabel;

        if (_phaseMarks == null) return;
        for (int i = 0; i < _phaseMarks.Length; i++)
            if (_phaseMarks[i] != null) _phaseMarks[i].SetActive(i == (int)phase);
    }

    public void SetResources(int gold, int food, int garrison)
    {
        if (_gold != null) _gold.text = gold.ToString();
        if (_food != null) _food.text = food.ToString();
        if (_garrison != null) _garrison.text = garrison.ToString();
    }

    public void SetActions(int left)
    {
        if (_actions != null) _actions.text = left > 0 ? $"Действий: {left}" : "Действий нет";
    }

    public void SetNavAvailable(bool map, bool court, bool castle)
    {
        if (_map != null) _map.interactable = map;
        if (_court != null) _court.interactable = court;
        if (_castle != null) _castle.interactable = castle;
    }
}
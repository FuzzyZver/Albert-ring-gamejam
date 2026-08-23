using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Вечерний экран. Заголовок, текст события и ряд кнопок выбора.
/// Кнопок в сцене положи столько, сколько может понадобиться (три с запасом) —
/// система включает нужное количество, остальные прячутся.
/// </summary>
public class EveningView : MonoBehaviour
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _body;
    [SerializeField] private Button[] _choices;
    [SerializeField] private TMP_Text[] _choiceLabels;

    public Button[] Choices => _choices;

    public void Show(string title, string body)
    {
        if (_title != null) _title.text = title;
        if (_body != null) _body.text = body;
    }

    public void SetChoices(string[] labels, int count)
    {
        if (_choices == null) return;

        for (int i = 0; i < _choices.Length; i++)
        {
            bool active = labels != null && i < count && i < labels.Length;
            if (_choices[i] != null) _choices[i].gameObject.SetActive(active);

            if (!active) continue;
            if (_choiceLabels != null && i < _choiceLabels.Length && _choiceLabels[i] != null)
                _choiceLabels[i].text = labels[i];
        }
    }
}
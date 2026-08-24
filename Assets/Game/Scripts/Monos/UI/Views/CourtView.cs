using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Тронный зал. Просители подходят по одному: кнопка «следующий», текст просьбы
/// и до трёх вариантов ответа. Кнопок в сцене положи три — система включит нужные.
/// </summary>
public class CourtView : MonoBehaviour
{
    [SerializeField] private TMP_Text _queue;        // «Проситель 2 из 3»
    [SerializeField] private TMP_Text _petitioner;
    [SerializeField] private TMP_Text _body;
    [SerializeField] private TMP_Text _result;

    [SerializeField] private Button _next;
    [SerializeField] private TMP_Text _nextLabel;

    [SerializeField] private Button[] _choices;
    [SerializeField] private TMP_Text[] _choiceLabels;
    [SerializeField] private TMP_Text[] _choiceHints;

    public Button NextButton => _next;
    public Button[] Choices => _choices;

    public void SetQueue(string text)
    {
        if (_queue != null) _queue.text = text;
    }

    public void ShowPetition(string petitioner, string body)
    {
        if (_petitioner != null) _petitioner.text = petitioner;
        if (_body != null) _body.text = body;
    }

    public void SetResult(string text)
    {
        if (_result == null) return;
        _result.text = text;
        _result.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    public void SetNext(bool visible, string label)
    {
        if (_next != null) _next.gameObject.SetActive(visible);
        if (_nextLabel != null) _nextLabel.text = label;
    }

    public void SetChoices(ChoiceDefinition[] choices, int count, int gold, int food, int garrison)
    {
        if (_choices == null) return;

        for (int i = 0; i < _choices.Length; i++)
        {
            bool active = choices != null && i < count && i < choices.Length && choices[i] != null;
            if (_choices[i] != null) _choices[i].gameObject.SetActive(active);
            if (!active) continue;

            var choice = choices[i];
            bool affordable = choice.CanAfford(gold, food, garrison);

            _choices[i].interactable = affordable;

            if (_choiceLabels != null && i < _choiceLabels.Length && _choiceLabels[i] != null)
                _choiceLabels[i].text = choice.Label;

            if (_choiceHints != null && i < _choiceHints.Length && _choiceHints[i] != null)
                _choiceHints[i].text = affordable ? choice.Hint() : choice.Missing(gold, food, garrison);
        }
    }
}
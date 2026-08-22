using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Строка глагола. Цвет выбирает сама — система не должна знать про палитру.
/// Строки лежат в сцене готовым пулом, ничего не инстанциируется в рантайме.
/// </summary>
public class VerbRowView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _breakdown;
    [SerializeField] private TMP_Text _result;

    [Header("Цвета результата")]
    [SerializeField] private Color _positive = new Color(0.36f, 0.80f, 0.45f);
    [SerializeField] private Color _negative = new Color(0.90f, 0.35f, 0.35f);
    [SerializeField] private Color _chance = new Color(0.35f, 0.60f, 0.95f);
    [SerializeField] private Color _blocked = new Color(0.45f, 0.45f, 0.45f);

    public Button Button => _button;
    public VerbId Verb { get; private set; }

    public void Set(VerbOutcome outcome)
    {
        gameObject.SetActive(true);
        Verb = outcome.Verb;

        if (_title != null) _title.text = outcome.Title;
        if (_cost != null) _cost.text = outcome.CostLine;

        if (_breakdown != null)
        {
            _breakdown.text = outcome.Available || string.IsNullOrEmpty(outcome.Blocked)
                ? outcome.Breakdown
                : outcome.Blocked;
        }

        if (_result != null)
        {
            _result.text = Result(outcome);
            _result.color = ResultColor(outcome);
        }

        if (_button != null) _button.interactable = outcome.Available;
    }

    public void Hide() => gameObject.SetActive(false);

    private static string Result(VerbOutcome outcome)
    {
        if (outcome.IsChanceBased) return outcome.Chance + "%";
        return outcome.Opinion > 0 ? "+" + outcome.Opinion : outcome.Opinion.ToString();
    }

    private Color ResultColor(VerbOutcome outcome)
    {
        if (!outcome.Available) return _blocked;
        if (outcome.IsChanceBased) return _chance;
        return outcome.Opinion >= 0 ? _positive : _negative;
    }
}
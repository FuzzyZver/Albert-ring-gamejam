using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Ночной счёт. Показывает предварительный итог: пока не лёг спать,
/// ползунки ещё можно двигать и цифры пересчитываются.
/// Ползунки настрой в инспекторе: Min 0, Max 3, Whole Numbers.</summary>
public class NightView : MonoBehaviour
{
    [Header("Ползунки")]
    [SerializeField] private Slider _peasantTax;
    [SerializeField] private Slider _lordTax;
    [SerializeField] private TMP_Text _peasantTaxLabel;
    [SerializeField] private TMP_Text _lordTaxLabel;

    [Header("Счёт")]
    [SerializeField] private TMP_Text _goldLine;
    [SerializeField] private TMP_Text _foodLine;
    [SerializeField] private TMP_Text _opinionLine;
    [SerializeField] private TMP_Text _warning;

    public Slider PeasantSlider => _peasantTax;
    public Slider LordSlider => _lordTax;

    public void SetSliders(int peasants, int lords)
    {
        if (_peasantTax != null) _peasantTax.SetValueWithoutNotify(peasants);
        if (_lordTax != null) _lordTax.SetValueWithoutNotify(lords);
    }

    public void SetReport(NightReportAttribute report, int goldAfter, int foodAfter, string warning)
    {
        if (_peasantTaxLabel != null)
            _peasantTaxLabel.text = $"Подать с крестьян: +{report.FoodIncome} пищи · {Signed(report.CommonsOpinionDelta)} им";
        if (_lordTaxLabel != null)
            _lordTaxLabel.text = $"Пошлина с лордов: +{report.GoldIncome} золота · {Signed(report.LordOpinionDelta)} им";

        if (_goldLine != null)
            _goldLine.text = $"Золото {Signed(report.GoldNet)}  →  {goldAfter}";
        if (_foodLine != null)
            _foodLine.text = $"Пища +{report.FoodIncome} − {report.FoodUpkeep} на гарнизон  →  {foodAfter}";
        if (_opinionLine != null)
        {
            _opinionLine.text = report.MemoryPenalty != 0
                ? $"Мнения: лорды {Signed(report.LordOpinionDelta)} · крестьяне {Signed(report.CommonsOpinionDelta)} (из них {report.MemoryPenalty} — старые обиды)"
                : $"Мнения: лорды {Signed(report.LordOpinionDelta)} · крестьяне {Signed(report.CommonsOpinionDelta)}";
        }

        if (_warning != null)
        {
            _warning.text = warning;
            _warning.gameObject.SetActive(!string.IsNullOrEmpty(warning));
        }
    }

    private static string Signed(int value) => value > 0 ? "+" + value : value.ToString();
}
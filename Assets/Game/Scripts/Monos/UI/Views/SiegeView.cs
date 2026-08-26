using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Экран осады: большая полоса противостояния внизу и строка разбора.
/// _barFill должен быть Image с типом Filled — заполнение и есть наша доля.</summary>
public class SiegeView : MonoBehaviour
{
    [SerializeField] private Image _barFill;
    [SerializeField] private TMP_Text _ourForce;
    [SerializeField] private TMP_Text _enemyForce;
    [SerializeField] private TMP_Text _summary;

    public void SetForces(int our, int enemy)
    {
        int total = Mathf.Max(1, our + enemy);
        if (_barFill != null) _barFill.fillAmount = our / (float)total;

        if (_ourForce != null) _ourForce.text = our.ToString();
        if (_enemyForce != null) _enemyForce.text = enemy.ToString();
    }

    public void SetSummary(string text)
    {
        if (_summary != null) _summary.text = text;
    }
}
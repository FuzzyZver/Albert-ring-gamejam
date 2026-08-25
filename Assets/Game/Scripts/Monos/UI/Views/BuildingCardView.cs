using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Карточка постройки: название на текущем уровне, сам уровень, что даёт,
/// и три кнопки уровней. Доступна всегда ровно одна — следующая по счёту.
/// </summary>
public class BuildingCardView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _close;

    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _level;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _bonus;
    [SerializeField] private TMP_Text _progress;

    [SerializeField] private Button[] _tiers;
    [SerializeField] private TMP_Text[] _tierLabels;
    [SerializeField] private TMP_Text[] _tierStates;

    public Button CloseButton => _close;
    public Button[] Tiers => _tiers;

    public void SetVisible(bool value)
    {
        if (_root != null) _root.SetActive(value);
    }

    public void Show(string title, int level, int maxLevel, string description, string bonus, string progress)
    {
        SetVisible(true);

        if (_title != null) _title.text = title;
        if (_level != null) _level.text = level > 0 ? $"{level} / {maxLevel}" : "не построено";
        if (_description != null) _description.text = description;
        if (_bonus != null) _bonus.text = string.IsNullOrEmpty(bonus) ? "пока ничего не даёт" : bonus;

        if (_progress == null) return;
        _progress.text = progress;
        _progress.gameObject.SetActive(!string.IsNullOrEmpty(progress));
    }

    /// <summary>Кнопка уровня активна только если это следующий уровень,
    /// стройка не идёт и хватает казны.</summary>
    public void SetTiers(BuildingDefinition definition, int level, bool building, int gold, int food)
    {
        if (_tiers == null) return;

        for (int i = 0; i < _tiers.Length; i++)
        {
            int tierLevel = i + 1;
            var tier = definition != null ? definition.Tier(tierLevel) : null;

            bool exists = tier != null;
            if (_tiers[i] != null) _tiers[i].gameObject.SetActive(exists);
            if (!exists) continue;

            bool next = tierLevel == level + 1;
            bool affordable = tier.CanAfford(gold, food);
            _tiers[i].interactable = next && !building && affordable;

            if (_tierLabels != null && i < _tierLabels.Length && _tierLabels[i] != null)
                _tierLabels[i].text = $"{tierLevel}. {tier.Title}";

            if (_tierStates == null || i >= _tierStates.Length || _tierStates[i] == null) continue;

            _tierStates[i].text =
                tierLevel <= level ? "построено" :
                building && tierLevel == level + 1 ? "строится" :
                !next ? "нужен уровень " + (tierLevel - 1) :
                !affordable ? "не хватает: " + tier.CostLine() :
                tier.CostLine();
        }
    }
}
using UnityEngine;

/// <summary>Окно выбора персонажа: корень + карточки кандидатов.</summary>
public class CharacterSelectView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private CandidateCardView[] _cards;

    public CandidateCardView[] Cards => _cards;

    public void SetVisible(bool value)
    {
        if (_root != null) _root.SetActive(value);
    }

    public void HideAllCards()
    {
        if (_cards == null) return;
        for (int i = 0; i < _cards.Length; i++) _cards[i].Hide();
    }
}
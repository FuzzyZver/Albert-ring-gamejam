using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Булавка внутри замка. В отличие от LordActor сущности не заводит:
/// у неё нет ни состояния, ни истории — только id слота и кнопка,
/// а заводить сущность ради этого незачем.
/// </summary>
public class CastleSlotView : MonoBehaviour
{
    [SerializeField] private CastleSlotId _slot;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _label;

    public CastleSlotId Slot => _slot;
    public Button Button => _button;

    public void SetLabel(string text)
    {
        if (_label != null) _label.text = text;
    }

    public void SetAvailable(bool value)
    {
        if (_button != null) _button.interactable = value;
    }
}
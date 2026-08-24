using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Точка доступа к UI из систем. Если у тебя тут уже есть свои поля — не заменяй файл,
/// а допиши недостающие.
/// </summary>
public class UI : MonoBehaviour
{
    [Header("Экраны")]
    public ScreensView Screens;          // Map / Court / Castle / Evening / Night
    public CharacterSelectView CharacterSelect;

    [Header("Полоса")]
    public HudView Hud;
    public Button NewRunButton;

    [Header("Карточка персонажа")]
    public LordCardView LordCard;
    public VerbPanelView VerbPanel;      // положи внутрь корня LordCard

    [Header("Двор")]
    public CourtView Court;

    [Header("Вечер")]
    public EveningView Evening;

    [Header("Замок и ночь")]
    public CastleSlotView[] CastleSlots;
    public NightView Night;

    [Header("Летопись и эпилог")]
    public ChronicleView Chronicle;
    public EpilogueView Epilogue;
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class UI_ConstructionSlotButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Data Configuration")]
    [SerializeField] private GameObject _constructionPrefab;
    [SerializeField] private ConstructionType _type;
    [SerializeField] private string _customName;

    [Header("References")]
    [SerializeField] private RectTransform _visualContainer;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _borderHighlight;

    public GameObject ConstructionPrefab => _constructionPrefab;
    public ConstructionType Type => _type;

    public event Action<UI_ConstructionSlotButton> OnSlotClicked;
    public event Action<UI_ConstructionSlotButton> OnSlotHovered;
    public event Action<UI_ConstructionSlotButton> OnSlotUnhovered;

    private Button _button;
    private bool _isFocused;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => OnSlotClicked?.Invoke(this));

        if (_borderHighlight != null)
        {
            _borderHighlight.enabled = false;
        }
    }

    private void Start()
    {
        UpdateSlotVisuals();
    }

    public void UpdateSlotVisuals()
    {
        if (_buttonText == null) return;

        if (string.IsNullOrEmpty(_customName))
        {
            _buttonText.text = _constructionPrefab.GetComponent<IBuildable>().Name;
        }
        else
        {
            _buttonText.text = _customName;
        }
    }
    public void SetSelectionState(bool isSelected)
    {
        _isFocused = isSelected;
        _borderHighlight.enabled = isSelected;
        // TODO: Animate scaling selected button
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnSlotHovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnSlotUnhovered?.Invoke(this);
    }
}
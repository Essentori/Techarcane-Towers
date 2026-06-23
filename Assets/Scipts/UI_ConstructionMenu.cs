using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConstructionMenu : UI_MenuBase
{
    [Serializable]
    public struct ConstructionTabMapping
    {
        public ConstructionType Type;
        public GameObject CategoryPanelContainer;
        public Button TabButton;
    }
    [Header("Resource UI Settings")]
    [SerializeField] private ResourceMapping[] _resourcesElements;

    private Dictionary<ResourceType, string> _resourceNames = new();

    [Header("Construction Categories Tabs Configuration")]
    [SerializeField] private ConstructionTabMapping[] _tabs;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _statsName;
    [SerializeField] private TextMeshProUGUI _statsDesription;
    [SerializeField] private Image _statsIcon;

    public event Action<GameObject, ConstructionType> OnConstructionConfirmed;

    private UI_ConstructionSlotButton[] _activeButtons;
    private int _currentSelectedIndex = -1;

    private void Awake()
    {
        foreach (var tab in _tabs)
        {
            ConstructionType capturedType = tab.Type;
            tab.TabButton.onClick.AddListener(() => SwitchToTab(capturedType));
        }
        foreach (var mapping in _resourcesElements)
        {
            if (mapping.ResourceName != null)
            {
                _resourceNames[mapping.Type] = mapping.ResourceName;
            }
        }
    }

    public override void OpenMenu()
    {
        base.OpenMenu();
        SwitchToTab(_tabs[0].Type);
    }

    protected override void SetMenuVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SwitchToTab(ConstructionType type)
    {
        UnsubscribeFromActiveButtons();

        foreach (var tab in _tabs)
        {
            bool isTargetTab = tab.Type == type;
            tab.CategoryPanelContainer.SetActive(isTargetTab);

            if (isTargetTab)
            {
                _activeButtons = tab.CategoryPanelContainer.GetComponentsInChildren<UI_ConstructionSlotButton>();
                SubscribeToActiveButtons();

                if (_activeButtons.Length > 0)
                {
                    SelectSlot(0);
                }
                else
                {
                    _currentSelectedIndex = -1;
                }
            }
        }
    }

    private void SelectSlot(int index)
    {
        if (_currentSelectedIndex >= 0 && _currentSelectedIndex < _activeButtons.Length)
        {
            _activeButtons[_currentSelectedIndex].SetSelectionState(false);
        }

        if (index < 0) index = _activeButtons.Length - 1;
        if (index >= _activeButtons.Length) index = 0;

        _currentSelectedIndex = index;

        _activeButtons[_currentSelectedIndex].SetSelectionState(true);
        UpdateStatsPanel(_activeButtons[_currentSelectedIndex]);
    }

    private void OnButtonClick(UI_ConstructionSlotButton button)
    {
        int clickedIndex = Array.IndexOf(_activeButtons, button);

        if (clickedIndex == _currentSelectedIndex)
        {
            ConfirmSelection();
        }
        else
        {
            SelectSlot(clickedIndex);
        }
    }

    private void OnButtonHovered(UI_ConstructionSlotButton button)
    {
        UpdateStatsPanel(button);
    }

    private void OnButtonUnhovered(UI_ConstructionSlotButton button)
    {
        if (_currentSelectedIndex >= 0 && _currentSelectedIndex < _activeButtons.Length)
        {
            UpdateStatsPanel(_activeButtons[_currentSelectedIndex]);
        }
    }

    public void ConfirmSelection()
    {
        var selectedButton = _activeButtons[_currentSelectedIndex];

        OnConstructionConfirmed?.Invoke(selectedButton.ConstructionPrefab, selectedButton.Type);
        CloseMenu();
    }

    private void UpdateStatsPanel(UI_ConstructionSlotButton button)
    {
        GameObject prefab = button.ConstructionPrefab;

        IBuildable buildable = prefab.GetComponent<IBuildable>();
        BlueprintStateController blueprint = prefab.GetComponent<BlueprintStateController>();

        string name = buildable.Name;
        string description = buildable.Description;
        string manaCost = blueprint.ManaCost.ToString();
        string resources = string.Empty;

        if (blueprint.ResourceCosts != null)
        {
            foreach (var req in blueprint.ResourceCosts)
            {
                resources += req.Amount + "x " + _resourceNames[req.Type] + "\n";
            }
        }
        description += "\nTo build:\n" + $"{manaCost}x mana\n" + resources; 

        //_statsIcon.sprite = 
        _statsName.text = name;
        _statsDesription.text = description;
    }

    private void SubscribeToActiveButtons()
    {
        foreach (var button in _activeButtons)
        {
            button.OnSlotClicked += OnButtonClick;
            button.OnSlotHovered += OnButtonHovered;
            button.OnSlotUnhovered += OnButtonUnhovered;
        }
    }

    private void UnsubscribeFromActiveButtons()
    {
        if (_activeButtons == null) return;

        foreach (var button in _activeButtons)
        {
            button.OnSlotClicked -= OnButtonClick;
            button.OnSlotHovered -= OnButtonHovered;
            button.OnSlotUnhovered -= OnButtonUnhovered;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromActiveButtons();
    }
}
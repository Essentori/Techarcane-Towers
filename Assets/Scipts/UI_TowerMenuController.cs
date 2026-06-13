using UnityEngine;
using UnityEngine.UIElements;

public class UI_TowerMenuController : UI_MenuBaseClass
{
    private UIDocument _uiDocument;
    private VisualElement _menuPanel;
    private Button _powerButton;
    private Button _exitButton;
    private Button _sellButton;
    private Label _towerNameLabel;
    private Tower _currentTower;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        _menuPanel = root.Q<VisualElement>("TowerMenuPanel");
        _powerButton = root.Q<Button>("PowerButton");
        _exitButton = root.Q<Button>("ExitButton");
        _sellButton = root.Q<Button>("SellButton");
        _towerNameLabel = root.Q<Label>("TowerNameLabel");

        _powerButton.RegisterCallback<ClickEvent>(OnPowerClicked);
        _exitButton.RegisterCallback<ClickEvent>(OnExitClicked);
        _sellButton.RegisterCallback<ClickEvent>(OnSellClicked);

        SetMenuVisibility(false);
    }

    protected override void SetMenuVisibility(bool visible)
    {
        if (_menuPanel != null)
        {
            _menuPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (!visible)
        {
            _currentTower = null;
        }
    }

    public void OpenMenu(Tower tower)
    {
        _currentTower = tower;
        if (_currentTower == null) return;

        if (_towerNameLabel != null)
        {
            _towerNameLabel.text = _currentTower.Name;
        }

        UpdatePowerButtonVisual();
        UpdateModuleSlotsVisual();

        base.OpenMenu();
    }

    private void OnPowerClicked(ClickEvent evt)
    {
        if (_currentTower == null) return;
        _currentTower.SwitchPowerState();
        UpdatePowerButtonVisual();
    }

    private void OnExitClicked(ClickEvent evt) => CloseMenu();

    private void OnSellClicked(ClickEvent evt)
    {
        if (_currentTower == null) return;

        Tower towerToDestroy = _currentTower;
        CloseMenu();
        Destroy(towerToDestroy.gameObject);
    }

    private void UpdatePowerButtonVisual()
    {
        if (_currentTower == null || _powerButton == null) return;

        if (_currentTower.IsOperational)
        {
            _powerButton.text = "ON";
            _powerButton.style.backgroundColor = new StyleColor(new Color(0.18f, 0.8f, 0.44f));
        }
        else
        {
            _powerButton.text = "OFF";
            _powerButton.style.backgroundColor = new StyleColor(new Color(0.92f, 0.26f, 0.21f));
        }
    }

    private void UpdateModuleSlotsVisual()
    {
    }
}
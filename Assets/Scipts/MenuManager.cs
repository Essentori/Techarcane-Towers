using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Menus List")]
    [SerializeField] private UI_MenuBase _mainMenu;
    [field: SerializeField] public UI_TowerMenuController TowerMenu { get; private set; }
    [field: SerializeField] public UI_ConstructionMenu ConstructionPick { get; private set; }

    [Header("Inventory (Sliding) Menus List")]
    [SerializeField] private List<UI_InventoryMenuBase> _inventoryMenus = new();

    private List<UI_MenuBase> _openMenus = new();
    private bool _isAnyMenuOpen => _openMenus.Count > 0;

    private void Start() => GameManager.Instance.Player.InputHandler.OnMenuPerformed += OnMenuPressed;

    private void OnDestroy() => GameManager.Instance.Player.InputHandler.OnMenuPerformed -= OnMenuPressed;

    public void RegisterMenu(UI_MenuBase menu)
    {
        if (!_openMenus.Contains(menu))
        {
            _openMenus.Add(menu);
            UpdateGameState();
        }
    }

    public void UnregisterMenu(UI_MenuBase menu)
    {
        if (_openMenus.Contains(menu))
        {
            _openMenus.Remove(menu);
            UpdateGameState();
        }
    }

    private void OnMenuPressed()
    {
        if (_isAnyMenuOpen)
        {
            _openMenus[_openMenus.Count - 1].CloseMenu();
        }
        else
        {
            if (_mainMenu != null)
            {
                _mainMenu.OpenMenu();
            }
        }
    }

    private void UpdateGameState()
    {
        GameManager.Instance.SetStateForMenu(_isAnyMenuOpen);
        if (_isAnyMenuOpen)
        {
            SetAllInventoriesState(UI_InventoryMenuBase.InventoryState.Disabled);
        }
        else
        {
            SetAllInventoriesState(UI_InventoryMenuBase.InventoryState.Collapsed);
        }
    }

    public void CloseAllCurrentMenus()
    {
        if (!_isAnyMenuOpen) return;
        foreach (var menu in _openMenus)
        {
            UnregisterMenu(menu);
        }
    }

    public void SetAllInventoriesState(UI_InventoryMenuBase.InventoryState newState)
    {
        foreach (var inventory in _inventoryMenus)
        {
            if (inventory != null)
            {
                inventory.ChangeState(newState);
            }
        }
    }
    public void SetSpecificInventoryState<T>(UI_InventoryMenuBase.InventoryState newState) where T : UI_InventoryMenuBase
    {
        foreach (var inventory in _inventoryMenus)
        {
            if (inventory is T)
            {
                inventory.ChangeState(newState);
                break;
            }
        }
    }
}
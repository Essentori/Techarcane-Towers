using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Menus List")]
    [SerializeField] private UI_MenuBaseClass _mainMenu;
    [SerializeField] private UI_TowerMenuController _towerMenu;
    [SerializeField] private UI_TowerSelectMenu _towerSelectMenu;

    private List<UI_MenuBaseClass> _openMenus = new List<UI_MenuBaseClass>();
    public bool IsAnyMenuOpen => _openMenus.Count > 0;

    public UI_TowerMenuController TowerMenu => _towerMenu;
    public UI_TowerSelectMenu TowerSelectMenu => _towerSelectMenu;
    void Update()
    {
        if (GameManager.Input.Player.Menu.triggered)
        {
            OnEscPressed();
        }
    }

    public void RegisterMenu(UI_MenuBaseClass menu)
    {
        if (!_openMenus.Contains(menu))
        {
            _openMenus.Add(menu);
            UpdateGameState();
        }
    }

    public void UnregisterMenu(UI_MenuBaseClass menu)
    {
        if (_openMenus.Contains(menu))
        {
            _openMenus.Remove(menu);
            UpdateGameState();
        }
    }

    private void OnEscPressed()
    {
        if (IsAnyMenuOpen)
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
        GameManager.Instance.SetStateForMenu(IsAnyMenuOpen);
    }
}
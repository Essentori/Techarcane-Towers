using UnityEngine;
using UnityEngine.InputSystem;

public class UI_TowerSelectMenu : UI_MenuBaseClass
{
    private GameObject _selectedTower;
    private UI_TowerSelectButton[] _buttons;
    private int _currentSelectedIndex = -1;

    private void Awake()
    {
        _buttons = GetComponentsInChildren<UI_TowerSelectButton>();
    }

    private void Start()
    {
        if (_buttons.Length > 0)
        {
            SelectSlot(0);
        }
    }

    private void Update()
    {
        HandleKeyboardInput();
        HandleScrollWheelInput();
    }

    public GameObject GetTower()
    {
        if (_selectedTower == null) PickFirstSlot();
        return _selectedTower;
    }

    public void OpenMenu(out GameObject towerObject)
    {
        _selectedTower = GetTower();
        towerObject = _selectedTower;
        base.OpenMenu();
    }

    protected override void SetMenuVisibility(bool visible)
    {
        if(!visible)
        {
            _currentSelectedIndex = 0;
            gameObject.SetActive(false);
        }
        gameObject.SetActive(visible);
    }

    private void HandleKeyboardInput()
    {

        for (int i = 0; i < _buttons.Length; i++)
        {
            Key targetKey = (Key)((int)Key.Digit1 + i);

            if (Keyboard.current[targetKey].wasPressedThisFrame)
            {
                SelectSlot(i);
                break;
            }
        }
    }

    private void HandleScrollWheelInput()
    {
        float scrollValue = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollValue) > 0.1f)
        {
            int newIndex = _currentSelectedIndex;
            newIndex = scrollValue > 0 ? -1 : +1;
            SelectSlot(newIndex);
        }
    }

    private void PickFirstSlot() => SelectSlot(0);

    public void SelectSlot(int index)
    {
        index = CycleIndex(index);
        _buttons[_currentSelectedIndex].SetFocus(false);
        _currentSelectedIndex = index;
        _buttons[_currentSelectedIndex].SetFocus(true);
        _selectedTower = _buttons[index].SlotTower;
    }

    private int CycleIndex(int index)
    {
        if (index < 0) return _buttons.Length - 1;
        if (index >= _buttons.Length) return 0;
        else return index;
    }
}
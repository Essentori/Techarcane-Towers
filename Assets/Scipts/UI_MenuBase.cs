using UnityEngine;

public abstract class UI_MenuBase : MonoBehaviour
{
    protected abstract void SetMenuVisibility(bool visible);

    public virtual void OpenMenu()
    {
        SetMenuVisibility(true);
        GameManager.Instance.Menus.RegisterMenu(this);
    }

    public virtual void CloseMenu()
    {
        SetMenuVisibility(false);
        GameManager.Instance.Menus.UnregisterMenu(this);
    }
}
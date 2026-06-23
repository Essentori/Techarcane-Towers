public interface IInteractable
{
    bool CanInteract();
    string GetInteractPrompt();
    void Interact();
    void DisplayOutline(bool show);
}
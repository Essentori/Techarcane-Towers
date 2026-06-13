using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private Camera _playerCamera;

    [Header("References")]
    [SerializeField] private GameObject _interactPromptContainer;
    [SerializeField] private TextMeshProUGUI _promptText;

    [Header("Settings")]
    [SerializeField] private float _interactDistanceFPV = 4f;
    [SerializeField] private float _interactDistanceTDV = 100f;

    private IInteractable _currentInteractable;
    private GameManager _hub;

    void Start()
    {
        _hub = GameManager.Instance;
        _playerCamera = GameManager.Instance.Player.PlayerCamera;
        _interactPromptContainer.SetActive(false);
    }

    void Update()
    {
        if (!_hub.Player.State.AllowInteract)
        {
            DisableCurrentInteractable();
            return;
        }
        if (_hub.Player.InputHandler.InteractPressed)
        {
            _currentInteractable.Interact();
        }

        Ray ray = _hub.Camera.GetCurrentLookRay();
        float rayDistance = _hub.Player.State.IsFPV ? _interactDistanceFPV : _interactDistanceTDV;

        if (Physics.Raycast(ray, out RaycastHit hit, _interactDistanceFPV, _hub.Layers.Interactible))
        {
            IInteractable[] interactables = hit.collider.GetComponentsInParent<IInteractable>();
            IInteractable targetInteractable = null;
            if (interactables != null)
            {
                DisableCurrentInteractable();
                return;
            }

            foreach (var interact in interactables)
            {
                if (interact.CanInteract())
                {
                    targetInteractable = interact;
                    break;
                }
            }

            if (targetInteractable == _currentInteractable) return;
            DisableCurrentInteractable();
            _currentInteractable = targetInteractable;
            _currentInteractable.DisplayOutline(true);

            _promptText.text = _currentInteractable.GetInteractPrompt();

            if (!_interactPromptContainer.activeSelf) _interactPromptContainer.SetActive(true);
        }

    }

    private void DisableCurrentInteractable()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.DisplayOutline(false);
            _currentInteractable = null;
        }

        if (_interactPromptContainer != null && _interactPromptContainer.activeSelf)
        {
            _interactPromptContainer.SetActive(false);
        }
    }
}
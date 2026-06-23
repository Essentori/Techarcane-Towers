using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GameObject _interactPromptContainer;
    [SerializeField] private TextMeshProUGUI _promptText;

    [Header("Settings")]
    [SerializeField] private float _interactDistanceFPV = 4f;
    [SerializeField] private float _interactDistanceTDV = 100f;

    private IInteractable _currentInteractable;
    private GameManager _hub;
    private PlayerController _player;

    private void Start()
    {
        _hub = GameManager.Instance;
        _player = GameManager.Instance.Player;
        _interactPromptContainer.SetActive(false);
        _currentInteractable = null;
        _hub.Player.InputHandler.OnAttackPerformed += DTVInteract;
    }
    private void OnDestroy()
    {
        _hub.Player.InputHandler.OnAttackPerformed -= DTVInteract;
    }

    void Update()
    {
        if (!_player.State.AllowInteract)
        {
            DisableCurrentInteractable();
            return;
        }
        if (_currentInteractable != null)
        {
            if(_player.State.IsFPV && _player.InputHandler.InteractPressed) _currentInteractable.Interact();
        }

        Ray ray = _hub.Camera.GetCurrentLookRay();
        float rayDistance = _player.State.IsFPV ? _interactDistanceFPV : _interactDistanceTDV;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, _hub.Layers.Interactable))
        {
            IInteractable[] interactables = hit.collider.GetComponentsInParent<IInteractable>();
            IInteractable targetInteractable = null;
            if (interactables == null || interactables.Length == 0)
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

            if (targetInteractable == null)
            {
                DisableCurrentInteractable();
                return;
            }

            if (targetInteractable == _currentInteractable) return;
            DisableCurrentInteractable();
            _currentInteractable = targetInteractable;
            _currentInteractable.DisplayOutline(true);

            _promptText.text = _currentInteractable.GetInteractPrompt();

            if (!_interactPromptContainer.activeSelf) _interactPromptContainer.SetActive(true);
        }
        else DisableCurrentInteractable();
    }

    private void DTVInteract()
    {
        if (!_player.State.AllowInteract || !_player.State.IsTDV || _currentInteractable == null) return;
        _currentInteractable.Interact();
    }

    private void DisableCurrentInteractable()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.DisplayOutline(false);
            _currentInteractable = null;
        }

        if (_interactPromptContainer.activeSelf)
        {
            _interactPromptContainer.SetActive(false);
        }
    }
}
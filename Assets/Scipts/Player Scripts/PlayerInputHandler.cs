using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private InputSystem_Actions.PlayerActions _input => GameManager.Input.Player;
    public Vector2 MoveInput => _input.Move.ReadValue<Vector2>();
    public Vector2 LookInput => _input.Look.ReadValue<Vector2>();

    public bool SprintPressed =>  _input.Sprint.IsPressed();
    public bool CrouchPressed => _input.Crouch.IsPressed();
    public bool JumpPressed =>  _input.Jump.IsPressed();
    public bool InteractPressed =>  _input.Interact.triggered;
    public bool AttackTriggered =>  _input.Attack.triggered;
    public bool AltAttackTriggered => _input.AltAttack.triggered;
    public bool ConstructionViewTriggered => _input.ToggleConstructionView.triggered;

    private void Start()
    {
        _input.BuildingMode.performed += HandleBuildingMode;
        _input.GridSnapping.performed += HandleGridSnapping;
        _input.RotateTower.performed += HandleRotateTower;
        _input.Jump.performed += HandleJump;
        _input.Rotate.performed += HandleCameraRotate;
        _input.Rotate.canceled += HandleCameraRotateCancel;
    }

    private void OnDestroy()
    {
        _input.BuildingMode.performed -= HandleBuildingMode;
        _input.GridSnapping.performed -= HandleGridSnapping;
        _input.RotateTower.performed -= HandleRotateTower;
        _input.Jump.performed -= HandleJump;
        _input.Rotate.performed -= HandleCameraRotate;
        _input.Rotate.canceled -= HandleCameraRotateCancel;
    }
    public event Action OnBuildingModePerformed;
    public event Action OnGridSnappingPerformed;
    public event Action OnRotateTowerPerformed;
    public event Action OnJumpPerformed;
    public event Action<float> OnCameraRotatePerformed;
    public event Action OnCameraRotateCanceled;

    private void HandleBuildingMode(InputAction.CallbackContext context) => OnBuildingModePerformed?.Invoke();

    private void HandleGridSnapping(InputAction.CallbackContext context) => OnGridSnappingPerformed?.Invoke();

    private void HandleRotateTower(InputAction.CallbackContext context) => OnRotateTowerPerformed?.Invoke();

    private void HandleJump(InputAction.CallbackContext context) => OnJumpPerformed?.Invoke();
    private void HandleCameraRotate(InputAction.CallbackContext context) => OnCameraRotatePerformed?.Invoke(context.ReadValue<float>());
    private void HandleCameraRotateCancel(InputAction.CallbackContext context) => OnCameraRotateCanceled?.Invoke();
}
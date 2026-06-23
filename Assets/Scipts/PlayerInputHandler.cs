using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private InputSystem_Actions.PlayerActions _input => GameManager.Input.Player;
    public Vector2 MoveInput => _input.Move.ReadValue<Vector2>();
    public Vector2 LookInput => _input.Look.ReadValue<Vector2>();

    public bool SprintIsPressed =>  _input.Sprint.IsPressed();
    public bool CrouchIsPressed => _input.Crouch.IsPressed();
    public bool JumpIsPressed => _input.Jump.IsPressed();
    public bool InteractPressed => _input.Interact.triggered;

    private void Start()
    {
        _input.BuildingMode.performed += HandleBuildingMode;
        _input.GridSnapping.performed += HandleGridSnapping;
        _input.RotateTower.performed += HandleRotateTower;
        _input.Jump.performed += HandleJump;
        _input.TDVRotate.performed += HandleTDVCameraRotate;
        _input.TDVRotate.canceled += HandleTDVCameraRotateCancel;
        _input.CameraSwitch.performed += HandleCameraSwitch;
        _input.Menu.performed += HandleMenu;
        _input.Attack.performed += HandleAttack;
        _input.AltAttack.performed += HandleAltAttack;
        _input.CursorUnlock.performed += CursorUnlock_performed;
        _input.CursorUnlock.canceled += CursorUnlock_canceled;
    }

    private void OnDestroy()
    {
        _input.BuildingMode.performed -= HandleBuildingMode;
        _input.GridSnapping.performed -= HandleGridSnapping;
        _input.RotateTower.performed -= HandleRotateTower;
        _input.Jump.performed -= HandleJump;
        _input.TDVRotate.performed -= HandleTDVCameraRotate;
        _input.TDVRotate.canceled -= HandleTDVCameraRotateCancel;
        _input.CameraSwitch.performed -= HandleCameraSwitch;
        _input.Menu.performed -= HandleMenu;
        _input.Attack.performed -= HandleAttack;
        _input.AltAttack.performed -= HandleAltAttack;
        _input.CursorUnlock.performed -= CursorUnlock_performed;
        _input.CursorUnlock.canceled -= CursorUnlock_canceled;
    }

    public event Action OnBuildingModePerformed;
    public event Action OnGridSnappingPerformed;
    public event Action OnRotateTowerPerformed;
    public event Action OnJumpPerformed;
    public event Action<float> OnCameraRotatePerformed;
    public event Action OnCameraRotateCanceled;
    public event Action OnCameraSwitchPerformed;
    public event Action OnMenuPerformed;
    public event Action OnAttackPerformed;
    public event Action OnAltAttackPerformed;
    public event Action<bool> OnCursorUnlockChanged;

    private void HandleBuildingMode(InputAction.CallbackContext context) => OnBuildingModePerformed?.Invoke();

    private void HandleGridSnapping(InputAction.CallbackContext context) => OnGridSnappingPerformed?.Invoke();

    private void HandleRotateTower(InputAction.CallbackContext context) => OnRotateTowerPerformed?.Invoke();

    private void HandleJump(InputAction.CallbackContext context) => OnJumpPerformed?.Invoke();
    private void HandleTDVCameraRotate(InputAction.CallbackContext context) => OnCameraRotatePerformed?.Invoke(context.ReadValue<float>());
    private void HandleTDVCameraRotateCancel(InputAction.CallbackContext context) => OnCameraRotateCanceled?.Invoke();
    private void HandleCameraSwitch(InputAction.CallbackContext context) => OnCameraSwitchPerformed?.Invoke();
    private void HandleMenu(InputAction.CallbackContext context) => OnMenuPerformed?.Invoke();
    private void HandleAttack(InputAction.CallbackContext context) => OnAttackPerformed?.Invoke();
    private void HandleAltAttack(InputAction.CallbackContext context) => OnAltAttackPerformed?.Invoke();
    private void CursorUnlock_performed(InputAction.CallbackContext context) => OnCursorUnlockChanged?.Invoke(true);
    private void CursorUnlock_canceled(InputAction.CallbackContext context) => OnCursorUnlockChanged?.Invoke(false);


}
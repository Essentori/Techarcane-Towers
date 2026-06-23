using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0f, 50f)] public float MoveSpeed = 6f;
    [Range(0.5f, 30f)] public float JumpHeight = 2f;

    [Header("Physics Settings")]
    [SerializeField] [Range(-50f, -9.8f)] private float _gravity = -20f;
    [SerializeField] [Range(0f, 10f)] private float _fallingGravityMultiplier = 2.5f;
    [SerializeField] private float _sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float _crouchSpeedMultiplier = 0.5f;

    [Header("Look Settings")]
    [Range(0f, 1000f)] public float MouseSensitivity = 100f;
    [SerializeField] [Range(-90f, 0f)] private float _upCameraClamp = -85f;
    [SerializeField] [Range(0f, 90f)] private float _downCameraClamp = 80f;

    private PlayerController _hub;
    private PlayerStatesManager _states;
    private CharacterController _controller;
    private PlayerInputHandler _input;

    private Vector3 _velocity;
    private Vector3 _movementVec;
    private float _cameraXRotation;
    private bool _wasGroundedLastFrame = true;
    private float _timeFalling;

    public CharacterController Controller => _controller;
    public bool IsMoving = false;
    public Vector3 Velocity => _velocity;
    public float TimeFalling => _timeFalling;
    public float CameraXRotation { get => _cameraXRotation; set => _cameraXRotation = value; }

    public event Action<float, float> OnPlayerLanded;
    public event Action OnPlayerJumped;

    void Awake()
    {
        _hub = GetComponent<PlayerController>();
        _controller = GetComponent<CharacterController>();
        _states = _hub.State;
        _input = _hub.InputHandler;
        ResetVerticalVelocity();
    }

    void Update()
    {
        ApplyGravity();
        if (_states.IsFPV)
        {
            LookHandle();
            MoveHandle();
        }
        else
        {
            _states.SetState(PlayerState.Sprinting, false);
            _states.SetState(PlayerState.Crouching, false);
            IsMoving = false;
            if (!_states.IsFlying)
            {
                HandleLanding();
                _controller.Move(_velocity * Time.deltaTime);
            }
        }
    }

    private void ApplyGravity()
    {
        if (_states.IsFlying)
        {
            _timeFalling = 0f;
            if (_velocity.y < 0f) _velocity.y = 0f;
            return;
        }
        if (_controller.isGrounded && _velocity.y < 0) return;

        float multiplier = _velocity.y < 1f ? _fallingGravityMultiplier : 1f;
        _velocity.y += _gravity * multiplier * Time.deltaTime;
    }

    private void LookHandle()
    {
        if (!_states.AllowLook) return;

        Vector2 look = _input.LookInput;
        float mouseX = look.x * MouseSensitivity * Time.unscaledDeltaTime;
        float mouseY = look.y * MouseSensitivity * Time.unscaledDeltaTime;

        _cameraXRotation = Mathf.Clamp(_cameraXRotation - mouseY, _upCameraClamp, _downCameraClamp);
        transform.Rotate(Vector3.up * mouseX);

        _hub.CameraPosition.localRotation = Quaternion.Euler(_cameraXRotation, 0f, 0f);
    }

    private void MoveHandle()
    {
        Vector2 moveVec = _states.AllowMovement ? _input.MoveInput : Vector2.zero;
        IsMoving = moveVec.sqrMagnitude > 0f;

        bool isSprinting = IsMoving && _input.SprintIsPressed;
        _states.SetState(PlayerState.Sprinting, isSprinting);

        bool isCrouching = _input.CrouchIsPressed && !_states.IsSprinting && _states.AllowCrouch;
        _states.SetState(PlayerState.Crouching, isCrouching);

        float speed = MoveSpeed * (_states.IsSprinting ? _sprintSpeedMultiplier : 1f);
        speed *= _states.IsCrouching? _crouchSpeedMultiplier : 1f;

        if (!_states.IsFlying)
        {
            HandleLanding();
            HandleJump();
        }

        Vector3 direction = transform.right * moveVec.x + transform.forward * moveVec.y;
        Vector3 finalMotion = direction * speed;

        if (!_states.IsFlying) finalMotion += _velocity;

        _controller.Move(finalMotion * Time.deltaTime);
    }

    private void HandleLanding()
    {
        if (!_controller.isGrounded)
        {
            _timeFalling += Time.deltaTime;
        }
        else if (_velocity.y < 0)
        {
            if (!_wasGroundedLastFrame && _timeFalling > 0.05f)
                OnPlayerLanded?.Invoke(_velocity.y, _timeFalling);

            _velocity.y = -3f;
            _timeFalling = 0f;
        }
        _wasGroundedLastFrame = _controller.isGrounded;
    }

    private void HandleJump()
    {
        if (_input.JumpIsPressed && _wasGroundedLastFrame && _timeFalling <= 0.1f && _states.AllowJump)
        {
            OnPlayerJumped?.Invoke();
            _velocity.y = Mathf.Sqrt(JumpHeight * -2f * _gravity);
        }
    }

    public void ResetVerticalVelocity() => _velocity = Vector3.zero;
}
using System;
using UnityEngine;

public class TDVController : MonoBehaviour
{
    private PlayerController _player;
    private CameraStatesManager _camera;

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 15f;
    [SerializeField] private float _moveLerpSpeed = 4f;
    [SerializeField] private float _rotateLerpSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 75f;
    [SerializeField] private bool _rotateInverted = true;

    private float _pitchAngle;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private float _currentYaw;
    private float _rawRotationInput = 0f;

    void Start()
    {
        _player = GameManager.Instance.Player;
        _camera = GameManager.Instance.Camera;

        _player.InputHandler.OnCameraRotatePerformed += RotatePerformed;
        _player.InputHandler.OnCameraRotateCanceled += RotateCanceled;

        _camera.OnCameraStateChanged += HandleCameraStateChanged;
    }

    void OnDestroy()
    {
        _player.InputHandler.OnCameraRotatePerformed -= RotatePerformed;
        _player.InputHandler.OnCameraRotateCanceled -= RotateCanceled;

        _camera.OnCameraStateChanged -= HandleCameraStateChanged;
    }

    private void RotatePerformed(float value) => _rawRotationInput = value;
    private void RotateCanceled() => _rawRotationInput = 0f;

    void Update()
    {
        TDVMovementHandle();
        TDVRotationHandle();
    }

    private void TDVMovementHandle()
    {
        Vector2 moveInput = _player.State.AllowMovement ? _player.InputHandler.MoveInput : Vector2.zero;

        Quaternion yawRotation = Quaternion.Euler(0, _currentYaw, 0);
        Vector3 forward = yawRotation * Vector3.forward;
        Vector3 right = yawRotation * Vector3.right;
        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

        _targetPosition += moveDirection * _moveSpeed * Time.deltaTime;

        transform.position = Vector3.Lerp(transform.position, _targetPosition, _moveLerpSpeed * Time.deltaTime);
    }

    private void TDVRotationHandle()
    {
        float yawChange = _player.State.AllowMovement ? _rawRotationInput : 0f;
        yawChange *= _rotateInverted ? 1f : -1f;
        _currentYaw += yawChange * _rotationSpeed * Time.deltaTime;
        _targetRotation = Quaternion.Euler(_pitchAngle, _currentYaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _rotateLerpSpeed * Time.deltaTime);
    }

    private void HandleCameraStateChanged(CamState state)
    {
        if (_camera.IsTDVCurrent)
        {
            enabled = true;
            _currentYaw = transform.eulerAngles.y;
            _targetPosition = transform.position;
            _pitchAngle = _camera.PitchAngle;
        }
        else enabled = false;
    }
}
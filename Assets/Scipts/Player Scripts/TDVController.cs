using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class TDVController : MonoBehaviour
{
    private GameManager _hub;
    private PlayerController _player;

    [Header("View Settings")]
    [SerializeField] private float _flyHeight = 12f;
    [SerializeField] private float _backwardOffset = 6f;
    [SerializeField][Range(10f, 60f)] private float _pitchAngle = 35f;

    [Header("Smoothness Settings")]
    [SerializeField] private float _moveSpeed = 15f;
    [SerializeField] private float _moveLerpSpeed = 4f;
    [SerializeField] private float _rotateLerpSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 75f;

    [Header("Animation Settings")]
    [SerializeField] private float _transitionUpSpeed = 3f;
    [SerializeField] private float _transitionDownSpeed = 1.5f;
    public float ReturnSpeedMultiplier = 1f;

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private float _currentYaw;
    private float _rawRotationInput = 0f;
    private Coroutine _transitionRoutine;


    private void Start()
    {
        _hub = GameManager.Instance;
        _player = _hub.Player;

        _player.InputHandler.OnCameraRotatePerformed += RotatePerformed;
        _player.InputHandler.OnCameraRotateCanceled += RotateCanceled;

        _hub.Camera.OnCameraStateChanged += ChangeToTDV;

        enabled = false;
    }

    private void OnDestroy()
    {
        _player.InputHandler.OnCameraRotatePerformed -= RotatePerformed;
        _player.InputHandler.OnCameraRotateCanceled -= RotateCanceled;
        _hub.Camera.OnCameraStateChanged -= ChangeToTDV;
    }

    private void RotatePerformed(float value) => _rawRotationInput = value;
    private void RotateCanceled() => _rawRotationInput = 0f;

    private void Update()
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
        if (_player.State.AllowMovement) return;
        _currentYaw += _rawRotationInput * _rotationSpeed * Time.deltaTime;
        _targetRotation = Quaternion.Euler(_pitchAngle, _currentYaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _rotateLerpSpeed * Time.deltaTime);
    }
    private void ChangeToTDV(CamState camState)
    {
        if (_transitionRoutine != null) StopCoroutine(_transitionRoutine);

        switch (camState)
        {
            case CamState.TransitionToTopDown:
                enabled = true;
                _transitionRoutine = StartCoroutine(TransitionToTopDownRoutine());
                break;

            case CamState.TransitionToFirstPerson:
                _transitionRoutine = StartCoroutine(TransitionToFirstPersonRoutine());
                break;

            case CamState.FirstPersonView:
                if (_player.Movement != null) _player.Movement.enabled = true;
                enabled = false;
                break;
        }
    }

    private IEnumerator TransitionToTopDownRoutine()
    {
        transform.SetParent(null);

        _currentYaw = _player.transform.eulerAngles.y;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        _targetPosition = CalculateStrategicPosition(_player.GetEyeWorldPosition());
        _targetRotation = Quaternion.Euler(_pitchAngle, _currentYaw, 0f);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * _transitionUpSpeed;
            float t = Mathf.Clamp01(elapsed);
            float smoothT = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, _targetPosition, smoothT);
            transform.rotation = Quaternion.Slerp(startRot, _targetRotation, smoothT);
            yield return null;
        }
        transform.position = _targetPosition;
        transform.rotation = _targetRotation;

        _hub.Camera.SwitchState(CamState.TopDownView);
    }
    private IEnumerator TransitionToFirstPersonRoutine()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float exitYaw = transform.eulerAngles.y;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * _transitionDownSpeed * ReturnSpeedMultiplier;
            float t = Mathf.Clamp01(elapsed);
            float easedT = t * t * t;

            Vector3 currentEyePos = _player.GetEyeWorldPosition();
            transform.position = Vector3.Lerp(startPos, currentEyePos, easedT);

            Quaternion targetFPVRot = Quaternion.Euler(_pitchAngle, exitYaw, 0f);
            transform.rotation = Quaternion.Slerp(startRot, targetFPVRot, easedT);
            yield return null;
        }

        _currentYaw = exitYaw;
        _player.transform.rotation = Quaternion.Euler(0f, exitYaw, 0f);

        if (_player.Movement != null)
        {
            _player.Movement.CameraXRotation = _pitchAngle;
        }

        _player.SnapCameraBackToPlayer();
        _hub.Camera.SwitchState(CamState.FirstPersonView);
    }
    private Vector3 CalculateStrategicPosition(Vector3 eyePos)
    {
        Quaternion yawRotation = Quaternion.Euler(_pitchAngle, _currentYaw, 0f);
        Vector3 offset = yawRotation * new Vector3(0, 0, -_backwardOffset);
        return new Vector3(eyePos.x, _flyHeight, eyePos.z) + offset;
    }
}
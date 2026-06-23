using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraStatesManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _crosshairObject;

    [Header("TDV Position Settings")]
    [SerializeField] private float _TDVHeight = 12f;
    [SerializeField] private float _offset = 6f;
    [SerializeField] [Range(30f, 90f)] public float PitchAngle = 45f;

    [Header("Transition Settings")]
    [SerializeField] private float _transitionSpeed = 3f;
    [SerializeField] private float _returnSpeedMultiplier = 1.5f;

    private CamState _currentState = CamState.FirstPersonView;
    private Camera _mainCamera => GameManager.Instance.MainCamera;
    public event Action<CamState> OnCameraStateChanged;

    private PlayerController _player => GameManager.Instance.Player;
    private HashSet<string> _cursorShowSources = new();
    private Coroutine _transitionRoutine;

    public bool IsFPVCurrent => _currentState == CamState.FirstPersonView;
    public bool IsTDVCurrent => _currentState == CamState.TransitionToTopDown || _currentState == CamState.TopDownView;

    private void Start() => _player.InputHandler.OnCameraSwitchPerformed += SwitchState;

    private void OnDestroy() => _player.InputHandler.OnCameraSwitchPerformed -= SwitchState;

    private void SwitchState()
    {
        if (_currentState == CamState.Locked) return;
        if (IsFPVCurrent) SetState(CamState.TransitionToTopDown);
        else SetState(CamState.TransitionToFirstPerson);
    }
    public void SetState(CamState newState)
    {
        if (_currentState == newState) goto changeValidation;
        if (_transitionRoutine != null) StopCoroutine(_transitionRoutine);

        switch (newState)
        {
            case CamState.Locked:
                HandleLockedCamera();
                break;

            case CamState.TransitionToTopDown:
                HandleToTDVTransition();
                break;

            case CamState.TopDownView:
                HandleTDVComplete();
                break;

            case CamState.TransitionToFirstPerson:
                HandleToFPVTransition();
                break;

            case CamState.FirstPersonView:
                HandleFPVComplete();
                break;
        }
        changeValidation:
        _currentState = newState;
        OnCameraStateChanged?.Invoke(newState);
        UpdateCrosshairState();
    }

    private void HandleLockedCamera()
    {
        UpdateCursorState(false, "CameraState");
    }

    private void HandleToTDVTransition()
    {
        UpdateCursorState(true, "CameraState");
        _transitionRoutine = StartCoroutine(TransitionToTopDownRoutine());
    }
    private void HandleToFPVTransition()
    {
        UpdateCursorState(false, "CameraState");
        _transitionRoutine = StartCoroutine(TransitionToFirstPersonRoutine());
    }

    private void HandleTDVComplete()
    {
        UpdateCursorState(true, "CameraState");
    }

    private void HandleFPVComplete() 
    {
        UpdateCursorState(false, "CameraState");
    }
    private IEnumerator TransitionToTopDownRoutine()
    {
        Transform camTransform = _mainCamera.transform;
        camTransform.SetParent(null);

        float currentYaw = _player.transform.eulerAngles.y;
        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;

        Vector3 targetPosition = CalculateTDVPosition(_player.GetEyeWorldPosition(), currentYaw);
        Quaternion targetRotation = Quaternion.Euler(PitchAngle, currentYaw, 0f);

        float progress = 0f;
        while (progress < 1f)
        {
            progress += _transitionSpeed * Time.deltaTime;
            float t = Mathf.Clamp01(progress);
            float smoothT = t * t * (3f - 2f * t);

            camTransform.position = Vector3.Lerp(startPos, targetPosition, smoothT);
            camTransform.rotation = Quaternion.Slerp(startRot, targetRotation, smoothT);
            yield return null;
        }

        camTransform.position = targetPosition;
        camTransform.rotation = targetRotation;

        SetState(CamState.TopDownView);
    }

    private IEnumerator TransitionToFirstPersonRoutine()
    {
        Transform camTransform = _mainCamera.transform;

        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;
        float exitYaw = camTransform.eulerAngles.y;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += _transitionSpeed * _returnSpeedMultiplier * Time.deltaTime;
            float t = Mathf.Clamp01(elapsed);
            float easedT = t * t * t;

            Vector3 currentEyePos = _player.GetEyeWorldPosition();
            camTransform.position = Vector3.Lerp(startPos, currentEyePos, easedT);

            Quaternion targetFPVRot = Quaternion.Euler(PitchAngle, exitYaw, 0f);
            camTransform.rotation = Quaternion.Slerp(startRot, targetFPVRot, easedT);
            yield return null;
        }

        _player.transform.rotation = Quaternion.Euler(0f, exitYaw, 0f);

        if (_player.Movement != null)
        {
            _player.Movement.CameraXRotation = PitchAngle;
        }

        _player.SnapCameraBackToPlayer();
        SetState(CamState.FirstPersonView);
    }

    private Vector3 CalculateTDVPosition(Vector3 eyePos, float yaw)
    {
        Quaternion yawRotation = Quaternion.Euler(PitchAngle, yaw, 0f);
        Vector3 offset = yawRotation * new Vector3(0, 0, -_offset);
        return new Vector3(eyePos.x, _TDVHeight, eyePos.z) + offset;
    }
    public void UpdateCursorState(bool toShow, string source)
    {
        if (toShow) _cursorShowSources.Add(source);
        else _cursorShowSources.Remove(source);

        bool cursorVisible = _cursorShowSources.Count > 0;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void UpdateCrosshairState()
    {
        _crosshairObject.SetActive(IsFPVCurrent);
    }

    public Ray GetCurrentLookRay()
    {
        if (_player.State.IsTDV)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            return _mainCamera.ScreenPointToRay(mousePosition);
        }
        else return _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }
}
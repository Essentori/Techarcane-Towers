using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _crosshairObject;
    private Camera _mainCamera => GameManager.Instance.MainCamera;
    public CamState CurrentState { get; private set; } = CamState.FirstPersonView;

    public event Action<CamState> OnCameraStateChanged;

    PlayerStatesManager _playerStates => GameManager.Instance.Player.State;
    private readonly HashSet<string> _cursorShowSources = new();

    public void SwitchState(CamState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;

        switch (CurrentState)
        {
            case CamState.Locked : HandleLockedCamera(); break;
            case CamState.TransitionToFirstPerson:
            case CamState.FirstPersonView : HandleFPV(); break;
            case CamState.TransitionToTopDown:
            case CamState.TopDownView: HandleTDV(); break;
            default: HandleCameraReset(); break;
        }
        OnCameraStateChanged?.Invoke(CurrentState);
    }
    private void HandleLockedCamera()
    {
        UpdateCursorState(false, "CameraState");
        _crosshairObject.SetActive(false);
    }
    private void HandleFPV()
    {
        UpdateCursorState(false, "CameraState");
        UpdateCrosshairState();
        _playerStates.SetState(PlayerState.FPV, true);
        GameManager.Instance.Player.SnapCameraBackToPlayer();
    }
    private void HandleTDV()
    {
        UpdateCursorState(true, "CameraState");
        UpdateCrosshairState();
        _playerStates.SetState(PlayerState.TDV, true);
    }
    private void HandleCameraReset()
    {
        _cursorShowSources.Clear();
        SwitchState(CamState.FirstPersonView);
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
        _crosshairObject.SetActive(_playerStates.IsFPV);
    }

    public Ray GetCurrentLookRay()
    {
        if (_playerStates.IsTDV)
        {
            Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            return _mainCamera.ScreenPointToRay(mousePosition);
        }
        else return _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }
}
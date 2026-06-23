using System.Collections.Generic;
using UnityEngine;

public enum MultLocked
{
    Locked = 0,
    Look = 1,
    Move = 2,
    Attack = 3,
    Interact = 4,
    Jump = 5,
    Crouch = 6
}

public class PlayerStatesManager : MonoBehaviour
{
    private HashSet<PlayerState> _activeStates = new();
    private HashSet<string>[] _lockSources;

    private CameraStatesManager _cameraState => GameManager.Instance.Camera;

    #region Static names for better readability
    private static PlayerState Locked = PlayerState.Locked;
    private static PlayerState Crouching = PlayerState.Crouching;
    private static PlayerState Sprinting = PlayerState.Sprinting;
    private static PlayerState Flying = PlayerState.Flying;
    private static PlayerState InBuildingMode = PlayerState.InBuildingMode;
    #endregion

    #region Data-Driven Rules Matrix
    private static Dictionary<PlayerState, PlayerState[]> _cancelRules = new()
    {
        { Sprinting,
            new[] { Crouching } },
         { Flying,
            new[] { Crouching } },
        { InBuildingMode,
            new[] { Crouching, Sprinting } },
        { Locked,
            new[] { Crouching, Sprinting } }
    };
    #endregion

    #region States Flags References
    public bool IsLocked => _lockSources[(int)MultLocked.Locked].Count != 0;
    public bool IsFlying => IsStateActive(Flying);
    public bool IsSprinting => IsStateActive(Sprinting);
    public bool IsCrouching => IsStateActive(Crouching);
    public bool IsBuildingModeActive => IsStateActive(InBuildingMode);
    public bool IsFPV => _cameraState.IsFPVCurrent;
    public bool IsTDV => _cameraState.IsTDVCurrent;
    public bool AllowLook => _lockSources[(int)MultLocked.Look].Count == 0;
    public bool AllowMovement => _lockSources[(int)MultLocked.Move].Count == 0 && !IsLocked;
    public bool AllowAttack => _lockSources[(int)MultLocked.Attack].Count == 0 && !IsBuildingModeActive && !IsLocked;
    public bool AllowInteract => _lockSources[(int)MultLocked.Interact].Count == 0 && !IsBuildingModeActive && !IsLocked;
    public bool AllowJump => _lockSources[(int)MultLocked.Jump].Count == 0 && !IsLocked;
    // TODO: Can crouch in fly mode, FIX!
    public bool AllowCrouch => _lockSources[(int)MultLocked.Crouch].Count == 0 && !IsLocked;
    #endregion
    public void ResetStates()
    {
        _activeStates.Clear();
        int totalTargets = System.Enum.GetNames(typeof(MultLocked)).Length;
        _lockSources = new HashSet<string>[totalTargets];
        for (int i = 0; i < totalTargets; i++) _lockSources[i] = new HashSet<string>();
    }
    public bool IsStateActive(PlayerState stateToCheck) => _activeStates.Contains(stateToCheck);
    private void ApplyState(PlayerState stateToApply)
    {
        if (_activeStates.Contains(stateToApply)) return;

        if (_cancelRules.TryGetValue(stateToApply, out var statesToCancel))
        {
            foreach (var state in statesToCancel)
            {
                _activeStates.Remove(state);
            }
        }

        _activeStates.Add(stateToApply);
    }

    private void RemoveState(PlayerState stateToRemove)
    {
        _activeStates.Remove(stateToRemove);
    }

    public void SetState(PlayerState stateToSet, bool toApply)
    {
        bool isApplied = IsStateActive(stateToSet);
        if (isApplied == toApply) return;

        if (toApply) ApplyState(stateToSet);
        else RemoveState(stateToSet);
    }

    public void SetLockSource(MultLocked target, bool toLock, string sourceHash)
    {
        int index = (int)target;

        if (toLock) _lockSources[index].Add(sourceHash);
        else _lockSources[index].Remove(sourceHash);
    }
}
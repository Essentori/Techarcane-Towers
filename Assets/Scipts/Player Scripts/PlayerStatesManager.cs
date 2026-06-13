using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatesManager : MonoBehaviour
{
    private readonly HashSet<PlayerState> _activeStates = new() { FPV, CanAttack, CanInteract };
    private readonly HashSet<string> _lookLockSources = new();
    private readonly HashSet<string> _moveLockSources = new();

    #region Static Init for better readability
    private static PlayerState Locked = PlayerState.Locked;
    private static PlayerState FPV = PlayerState.FPV;
    private static PlayerState TDV = PlayerState.TDV;
    private static PlayerState Crouching = PlayerState.Crouching;
    private static PlayerState Sprinting = PlayerState.Sprinting;
    private static PlayerState Flying = PlayerState.Flying;
    private static PlayerState CanInteract = PlayerState.CanInteract;
    private static PlayerState CanAttack = PlayerState.CanAttack;
    private static PlayerState InBuildingMode = PlayerState.InBuildingMode;
    #endregion

    #region Data-Driven Rules Matrix
    private static readonly Dictionary<PlayerState, PlayerState[]> _cancelRules = new()
    {
        { FPV, 
            new[] { TDV } },
        { TDV, 
            new[] { FPV, Crouching, Sprinting } },
        { InBuildingMode, 
            new[] { CanAttack, CanInteract } },
        { Locked, 
            new[] { Crouching, Sprinting, CanInteract, CanAttack } }
    };
    #endregion

    #region States Flags
    public bool IsMoving => AllowMovement ? GameManager.Instance.Player.InputHandler.MoveInput.sqrMagnitude > 0.001f : false;
    public bool IsFlying => IsStateActive(Flying);
    public bool IsSprinting => IsStateActive(Sprinting);
    public bool IsCrouching => IsStateActive(Crouching);
    public bool IsFPV => IsStateActive(FPV);
    public bool IsTDV => IsStateActive(TDV);
    public bool AllowLook => _lookLockSources.Count > 0;
    public bool AllowMovement => _moveLockSources.Count > 0;
    public bool AllowJump => AllowMovement && !IsStateActive(Crouching);
    public bool AllowInteract => !IsStateActive(InBuildingMode);
    #endregion

    public bool IsStateActive(PlayerState stateToCheck) => _activeStates.Contains(stateToCheck);
    public void ApplyState(PlayerState stateToApply)
    {
        if (_activeStates.Contains(stateToApply)) return;

        if (_cancelRules.TryGetValue(stateToApply, out var statesToCancel))
        {
            foreach (var state in statesToCancel)
            {
                _activeStates.Remove(state);
            }
        }
    }

    public void RemoveState(PlayerState stateToRemove)
    {
        _activeStates.Remove(stateToRemove);
    }

    public void SetState(PlayerState stateToCheck, bool toApply)
    {
        bool isApplied = IsStateActive(stateToCheck);
        if (isApplied == toApply) return;

        if (toApply) ApplyState(stateToCheck);
        else RemoveState(stateToCheck);
    }
    public void SetLookLock(bool toLock, string sourceHash)
    {
        if (toLock) _lookLockSources.Append(sourceHash);
        else _lookLockSources.Remove(sourceHash);
    }
    public void SetMoveLock(bool toLock, string sourceHash)
    {
        if (toLock) _moveLockSources.Append(sourceHash);
        else _moveLockSources.Remove(sourceHash);
    }

}
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFlyController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _flyVerticalSpeed = 8f;
    [SerializeField] private float _doubleTapTimeWindow = 0.25f;

    private PlayerController _player;
    private CharacterController _controller;

    private int _jumpTapCount = 0;
    private Coroutine _resetTapRoutine;

    private Vector3 _flyVerticalVelocity;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _player = GetComponent<PlayerController>();
    }
    private void Start() => _player.InputHandler.OnJumpPerformed += OnJumpPressed;

    private void OnDestroy() => _player.InputHandler.OnJumpPerformed -= OnJumpPressed;
    private void OnJumpPressed()
    {
        _jumpTapCount++;

        if (_jumpTapCount == 1)
        {
            _resetTapRoutine = StartCoroutine(ResetTapWindow());
        }
        else if (_jumpTapCount == 2)
        {
            if (_player.State.IsFlying)
                ExitFlyMode();
            else
                EnterFlyMode();

            ResetTaps();
        }
    }
    private IEnumerator ResetTapWindow()
    {
        yield return new WaitForSeconds(_doubleTapTimeWindow);
        _jumpTapCount--;
    }

    private void ResetTaps()
    {
        if (_resetTapRoutine != null) StopCoroutine(_resetTapRoutine);
        _jumpTapCount = 0;
    }

    void Update()
    {
        if (!_player.State.IsFlying) return;
        if (_controller.isGrounded)
        {
            ExitFlyMode();
            return;
        }
        if (_player.State.AllowMovement)
        {
            HandleFlyVerticalMovement();
        }
        else
        {
            _controller.Move(Vector3.zero);
        }
    }
    private void EnterFlyMode()
    {
        _player.State.ApplyState(PlayerState.Flying);
        _flyVerticalVelocity = Vector3.zero;
        _player.Movement.ResetVerticalVelocity();
    }

    public void ExitFlyMode()
    {
        _player.State.RemoveState(PlayerState.Flying);
        _flyVerticalVelocity = Vector3.zero;
    }

    private void HandleFlyVerticalMovement()
    {
        float verticalDirection = 0f;
        if (_player.InputHandler.JumpPressed) verticalDirection += 1f;
        if (_player.InputHandler.CrouchPressed) verticalDirection -= 1f;

        _flyVerticalVelocity.y = verticalDirection * _flyVerticalSpeed;
        _controller.Move(_flyVerticalVelocity * Time.deltaTime);
    }
}
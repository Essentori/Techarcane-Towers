using DG.Tweening;
using UnityEngine;

public class FPVCameraEffects : MonoBehaviour
{
    // TODO: Improve readability
    private Camera _playerCamera;
    private Transform _cameraObject;
    private PlayerController _player;
    private PlayerMovement _movement;

    [Header("FOV increase settings")]
    public float BaseFOV_FPV = 75f;
    [SerializeField][Range(1.1f, 2f)] private float _FOV_IncreaseMultiplier = 1.4f;
    [SerializeField][Range(0.5f, 5f)] private float _FOV_IncreaseTime = 2f;
    [SerializeField] private float _FOV_SprintMultiplier = 1.12f;
    [SerializeField] private float _FOV_TransitionSpeed = 8f;

    [Header("Head Bob Settings")]
    [SerializeField] private float _bobSpeedThreshold = 0.1f;
    [SerializeField] private float _bobSpeed = 14f;
    [SerializeField] private float _bobHorizontalAmount = 0.05f;
    [SerializeField] private float _bobVerticalAmount = 0.05f;
    [SerializeField] private float _bobReturnSpeed = 8f;

    [Header("Fall Shacking Settings")]
    [SerializeField] private float _fallShakeFrequency = 15f;
    [SerializeField] private float _fallShakeTranslationAmount = 0.08f;
    [SerializeField] private float _fallShakeRotationAmount = 1.8f;
    [SerializeField] private float _shakeSmoothSpeed = 10f;

    [Header("Landing Impact Settings")]
    [SerializeField] private float _fallVelocityThershold = 1f;
    [SerializeField] private float _minImpactDisplacement = 0.08f;
    [SerializeField] private float _maxImpactDisplacement = 0.25f;
    [SerializeField] private float _maxFallTimeForImpact = 1.2f;

    private float _targetFOV;
    private float _fallFOVTimer;
    private float _bobCycleTimer;

    private float _animatedY = 0f;
    private bool _wasSteppingLastFrame;

    private Vector3 _currentBobOffset;
    private Vector3 _currentShakeOffset;
    private float _currentShakeRoll;

    private Sequence _landingSequence;
    private float _landingTiltX = 0f;

    void Awake()
    {
        _player = GetComponent<PlayerController>();
        _cameraObject = _player.CameraPosition;
        _playerCamera = _player.PlayerCamera;
        _movement = _player.Movement;
    }

    void Start()
    {
        _targetFOV = BaseFOV_FPV;
        _animatedY = 0f;

        _movement.OnPlayerLanded += ExecuteLandingImpact;
        _movement.OnPlayerJumped += ExecuteJumpImpact;
        GameManager.Instance.Camera.OnCameraStateChanged += HandleStateChange;
    }
    void LateUpdate()
    {
        if (!_player.State.IsFPV) return;

        AnimateCameraEffects();
        HandleFOV();

        _cameraObject.localRotation = Quaternion.Euler(_movement.CameraXRotation + _landingTiltX, 0f, _currentShakeRoll);
    }

    void HandleFOV()
    {
        float inputFOV = _player.State.IsSprinting ? (BaseFOV_FPV * _FOV_SprintMultiplier) : BaseFOV_FPV;

        if (_movement.Controller.isGrounded)
        {
            _targetFOV = inputFOV;
            _fallFOVTimer = 0f;
        }
        else
        {
            if (_movement.Velocity.y < -_fallVelocityThershold && !_player.State.IsFlying)
            {
                _fallFOVTimer = Mathf.Clamp(_fallFOVTimer + Time.deltaTime, 0f, _FOV_IncreaseTime);
                float fallProgress = _fallFOVTimer / _FOV_IncreaseTime;
                float maxFallFOV = BaseFOV_FPV * _FOV_IncreaseMultiplier;
                _targetFOV = Mathf.Lerp(inputFOV, maxFallFOV, fallProgress);
            }
            else
            {
                _targetFOV = inputFOV;
                _fallFOVTimer = 0f;
            }
        }

        _playerCamera.fieldOfView = Mathf.Lerp(_playerCamera.fieldOfView, _targetFOV, _FOV_TransitionSpeed * Time.deltaTime);
    }

    void AnimateCameraEffects()
    {
        Vector3 horizontalVelocity = new Vector3(_movement.Controller.velocity.x, 0f, _movement.Controller.velocity.z);
        float playerActualSpeed = horizontalVelocity.magnitude;

        bool isStepping = _movement.Controller.isGrounded && _movement.IsMoving;

        if (isStepping)
        {
            if (!_wasSteppingLastFrame)
            {
                _bobCycleTimer = Random.value > 0.5f ? 0f : Mathf.PI;
            }
            float baseWalkSpeed = _player.Movement.MoveSpeed;
            float speedRatio = baseWalkSpeed > _bobSpeedThreshold ? (playerActualSpeed / baseWalkSpeed) : 0f;
            float frequencyFactor = Mathf.Clamp(speedRatio, 0.5f, 1.8f);

            _bobCycleTimer += frequencyFactor * _bobSpeed * Time.deltaTime;

            float amplitudeFactor = Mathf.Clamp(speedRatio, 0.2f, 1.3f);

            float bobX = Mathf.Sin(_bobCycleTimer * 0.5f) * _bobHorizontalAmount * amplitudeFactor;
            float bobY = Mathf.Sin(_bobCycleTimer) * _bobVerticalAmount * amplitudeFactor;

            _currentBobOffset = new Vector3(bobX, bobY, 0f);
        }
        else
        {
            _bobCycleTimer = 0f;
            _currentBobOffset = Vector3.Lerp(_currentBobOffset, Vector3.zero, _bobReturnSpeed * Time.deltaTime);
        }

        _wasSteppingLastFrame = isStepping;

        CameraFallShacking();
    }

    private void CameraFallShacking()
    {
        Vector3 targetShakeOffset = Vector3.zero;
        float targetShakeRoll = 0f;

        if (!_movement.Controller.isGrounded && _movement.Velocity.y < -2f && !_player.State.IsFlying)
        {
            float fallSpeed = Mathf.InverseLerp(-2f, -18f, _movement.Velocity.y);
            float fallTime = Mathf.Clamp01(_movement.TimeFalling / _maxFallTimeForImpact);
            float totalShakeProgress = fallSpeed * Mathf.Lerp(0.2f, 1.0f, fallTime);

            float noiseSeed = Time.time * _fallShakeFrequency;
            float shakeX = (Mathf.PerlinNoise(noiseSeed, 4.5f) - 0.5f) * 2f * _fallShakeTranslationAmount * totalShakeProgress;
            float shakeY = (Mathf.PerlinNoise(8.5f, noiseSeed) - 0.5f) * 2f * _fallShakeTranslationAmount * totalShakeProgress;

            targetShakeOffset = new Vector3(shakeX, shakeY, 0f);
            targetShakeRoll = (Mathf.PerlinNoise(noiseSeed, noiseSeed + 12.5f) - 0.5f) * 2f * _fallShakeRotationAmount * totalShakeProgress;
        }

        float currentSmooth = (targetShakeOffset == Vector3.zero) ? _bobReturnSpeed : _shakeSmoothSpeed;
        _currentShakeOffset = Vector3.Lerp(_currentShakeOffset, targetShakeOffset, currentSmooth * Time.deltaTime);
        _currentShakeRoll = Mathf.Lerp(_currentShakeRoll, targetShakeRoll, currentSmooth * Time.deltaTime);

        float currentDynamicEyeHeight = _player.LocalEyeHeight;

        _cameraObject.localPosition = new Vector3(
            _currentBobOffset.x + _currentShakeOffset.x,
            currentDynamicEyeHeight + _animatedY + _currentBobOffset.y + _currentShakeOffset.y,
            0f
        );
    }

    private void ExecuteLandingImpact(float landingVelocity, float timeFalling)
    {
        float fallProgress = Mathf.Clamp01(timeFalling / _maxFallTimeForImpact);
        float currentDisplacement = Mathf.Lerp(_minImpactDisplacement, _maxImpactDisplacement, fallProgress);

        float targetLandY = -currentDisplacement;

        float maxJumpLandingVelocity = -Mathf.Sqrt(_movement.JumpHeight * 2f * Mathf.Abs(-20f) * 2.5f);
        float tiltThreshold = maxJumpLandingVelocity - 5f;

        float targetTilt = 0f;
        if (landingVelocity < tiltThreshold)
        {
            float tiltProgress = Mathf.InverseLerp(tiltThreshold, maxJumpLandingVelocity - 15f, landingVelocity);
            targetTilt = Mathf.Lerp(0f, 15f, tiltProgress);
        }

        float downDuration = Mathf.Lerp(0.05f, 0.1f, fallProgress);
        float returnDuration = Mathf.Lerp(0.15f, 0.55f, fallProgress);

        _landingSequence?.Kill();
        _landingSequence = DOTween.Sequence();

        _landingSequence.Append(DOTween.To(() => _animatedY, x => _animatedY = x, targetLandY, downDuration).SetEase(Ease.OutQuad));
        _landingSequence.Join(DOTween.To(() => _landingTiltX, x => _landingTiltX = x, targetTilt, downDuration).SetEase(Ease.OutQuad));

        if (fallProgress > 0.5f)
        {
            _landingSequence.AppendInterval(Mathf.Lerp(0f, 0.08f, fallProgress));
        }

        _landingSequence.Append(DOTween.To(() => _animatedY, x => _animatedY = x, 0f, returnDuration).SetEase(Ease.OutCubic));
        _landingSequence.Join(DOTween.To(() => _landingTiltX, x => _landingTiltX = x, 0f, returnDuration).SetEase(Ease.OutCubic));
    }

    private void ExecuteJumpImpact()
    {
        float currentTilt = _landingTiltX;

        _landingSequence?.Kill();
        _landingSequence = DOTween.Sequence();

        _landingSequence.Append(DOTween.To(() => _animatedY, x => _animatedY = x, -_minImpactDisplacement, 0.08f).SetEase(Ease.OutQuad));

        if (Mathf.Abs(currentTilt) > 0.001f)
        {
            _landingSequence.Join(DOTween.To(() => _landingTiltX, x => _landingTiltX = x, 0f, 0.25f).SetEase(Ease.OutCubic));
        }
        else
        {
            _landingTiltX = 0f;
        }

        _landingSequence.Append(DOTween.To(() => _animatedY, x => _animatedY = x, 0f, 0.2f).SetEase(Ease.InOutQuad));
    }

    private void HandleStateChange(CamState state)
    {
        if (state == CamState.TransitionToTopDown)
        {
            ResetEffects();
        }
        if (state == CamState.FirstPersonView) enabled = true;
        else enabled = false;
    }

    private void ResetEffects()
    {
        _landingSequence?.Kill();
        _animatedY = 0f;
        _landingTiltX = 0f;
        _currentBobOffset = Vector3.zero;
        _currentShakeOffset = Vector3.zero;
        _currentShakeRoll = 0f;
    }

    void OnDestroy()
    {
        _player.Movement.OnPlayerLanded -= ExecuteLandingImpact;
        _player.Movement.OnPlayerJumped -= ExecuteJumpImpact;
        GameManager.Instance.Camera.OnCameraStateChanged -= HandleStateChange;
    }
}
using UnityEngine;
using DG.Tweening;

public class ModulePickup : MonoBehaviour, IInteractable
{
    private enum ModuleState { Spawning, OnGround, BouncingUp, Flying }
    private ModuleState _currentState = ModuleState.Spawning;

    private object _storedModuleData;
    private bool _isPickedInTDV = false;

    [Header("Distance Settings")]
    [SerializeField] private float _highlightRadius = 10f;
    [SerializeField] private float _tdvHighlightRadius = 5f;
    [SerializeField] private float _pickupRadius = 3f;

    [Header("Pickup Animation Settings")]
    [SerializeField] private float _flyDuration = 0.65f;

    private float _flightProgress = 0f;
    private Vector3 _flightStartPos;
    private Vector3 _flightControlPoint;

    private Rigidbody _rb;
    private Collider _physicalCollider;
    private InteractableOutline _outline;

    private Transform _playerTransform;
    private Camera _mainCamera;

    private Sequence _bounceSequence;
    private TrailRenderer _trail;

    private static int _lastCheckedFrame = -1;
    private static Vector3 _cachedTDVCursorHitPoint;
    private static bool _isHitValid;

    public void Initialize(object moduleData)
    {
        _storedModuleData = moduleData;

        _rb = GetComponent<Rigidbody>();
        _physicalCollider = GetComponent<Collider>();
        _outline = GetComponent<InteractableOutline>();
        _trail = GetComponentInChildren<TrailRenderer>();

        _mainCamera = GameManager.Instance.MainCamera;
        _playerTransform = GameManager.Instance.Player.transform;

        SpawningAnimation();
    }

    private void OnDestroy()
    {
        if (_bounceSequence != null)
        {
            _bounceSequence.Kill();
            _bounceSequence = null;
        }
    }

    private void Start()
    {
        if (_storedModuleData == null && GameManager.Instance != null)
        {
            Initialize(null);
        }
    }

    private void Update()
    {
        if (_playerTransform == null || _mainCamera == null) return;

        if (_currentState == ModuleState.Spawning || _currentState == ModuleState.OnGround)
        {
            if (GameManager.Instance.Camera.IsTDVCurrent)
            {
                UpdateTDVCursorHitPoint();
                TDVProximityCheck();
            }
            else
            {
                float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
                DistanceCheck(distanceToPlayer);
            }
        }
        else if (_currentState == ModuleState.Flying)
        {
            ParabolicFlight();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_currentState == ModuleState.Spawning)
        {
            _currentState = ModuleState.OnGround;
        }
    }

    private void SpawningAnimation()
    {
        _currentState = ModuleState.Spawning;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            Vector3 pushDirection = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(3.5f, 5f), Random.Range(-1.5f, 1.5f));
            _rb.AddForce(pushDirection, ForceMode.Impulse);

            Vector3 randomTorque = new Vector3(Random.Range(-8f, 8f), Random.Range(-8f, 8f), Random.Range(-8f, 8f));
            _rb.AddTorque(randomTorque, ForceMode.Impulse);
        }
    }

    private void DistanceCheck(float distanceToPlayer)
    {
        if (_outline != null)
        {
            bool shouldHighlight = distanceToPlayer <= _highlightRadius;
            _outline.ToggleOutline(shouldHighlight);
        }

        if (distanceToPlayer <= _pickupRadius)
        {
            StartPickupSequence(false);
        }
    }

    private void UpdateTDVCursorHitPoint()
    {
        if (Time.frameCount == _lastCheckedFrame) return;

        _lastCheckedFrame = Time.frameCount;
        Ray ray = GameManager.Instance.Camera.GetCurrentLookRay();
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, GameManager.Instance.Layers.Walkable))
        {
            _cachedTDVCursorHitPoint = hit.point;
            _isHitValid = true;
        }
        else
        {
            _isHitValid = false;
        }
    }

    private void TDVProximityCheck()
    {
        if (_outline == null) return;

        if (_isHitValid)
        {
            float distanceToHit = Vector3.Distance(transform.position, _cachedTDVCursorHitPoint);
            bool shouldHighlight = distanceToHit <= _tdvHighlightRadius;
            _outline.ToggleOutline(shouldHighlight);
        }
        else
        {
            _outline.ToggleOutline(false);
        }
    }

    private void StartPickupSequence(bool isTDV)
    {
        _currentState = ModuleState.BouncingUp;
        _isPickedInTDV = isTDV;

        if (_rb != null) _rb.isKinematic = true;
        if (_physicalCollider != null) _physicalCollider.enabled = false;
        if (_outline != null) _outline.ToggleOutline(false);

        _bounceSequence?.Kill();
        _bounceSequence = DOTween.Sequence();

        Vector3 peakPosition = transform.position + Vector3.up * 1.5f;
        float startY = transform.eulerAngles.y;

        _bounceSequence.Append(transform.DOMove(peakPosition, 0.35f).SetEase(Ease.OutQuad));
        _bounceSequence.Join(transform.DORotate(new Vector3(0f, startY + 180f, 90f), 0.35f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));

        _bounceSequence.Append(transform.DORotate(new Vector3(0f, startY + 180f + 360f, 0.45f), 0.45f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine));
        _bounceSequence.AppendInterval(0.1f);

        _bounceSequence.OnComplete(() =>
        {
            _flightStartPos = transform.position;
            _flightProgress = 0f;

            Vector3 targetPos = GetCurrentTargetPosition();

            Vector3 directionToTarget = (targetPos - _flightStartPos).normalized;
            _flightControlPoint = _flightStartPos + (Vector3.down * 1.5f) + (directionToTarget * 2.0f);

            _currentState = ModuleState.Flying;
        });
    }

    private void ParabolicFlight()
    {
        _flightProgress += Time.deltaTime / _flyDuration;
        if (_flightProgress > 1f) _flightProgress = 1f;

        Vector3 targetPos = GetCurrentTargetPosition();

        Vector3 midPoint = Vector3.Lerp(_flightStartPos, targetPos, _flightProgress);
        Vector3 currentControl = Vector3.Lerp(_flightControlPoint, midPoint, _flightProgress);

        Vector3 m1 = Vector3.Lerp(_flightStartPos, currentControl, _flightProgress);
        Vector3 m2 = Vector3.Lerp(currentControl, targetPos, _flightProgress);

        transform.position = Vector3.Lerp(m1, m2, _flightProgress);
        transform.Rotate(Vector3.up * 450f * Time.deltaTime, Space.Self);
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, _flightProgress);

        if (_flightProgress >= 1f)
        {
            CompletePickup();
        }
    }

    private Vector3 GetCurrentTargetPosition()
    {
        if (_isPickedInTDV)
        {
            return _playerTransform.position + Vector3.up * 1f;
        }
        else
        {
            return _mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.02f, 1.2f));
        }
    }

    private void CompletePickup()
    {
        if (_trail != null)
        {
            _trail.Clear();
            _trail.enabled = false;
        }
        var inventory = GameManager.Instance.Player.Inventory;
        if (_storedModuleData is ModuleConditionTypeData conditionData)
        {
            inventory.AddConditionModule(conditionData);
            Debug.Log($"[Inventory] Added condition module: {conditionData.name}");
        }
        else if (_storedModuleData is ModuleActionTypeData actionData)
        {
            inventory.AddActionModule(actionData);
            Debug.Log($"[Inventory] Added action module: {actionData.name}");
        }
        Destroy(gameObject);
    }

    public bool CanInteract() => _currentState == ModuleState.Spawning || _currentState == ModuleState.OnGround;
    public string GetInteractPrompt() => string.Empty;
    public void Interact() { }

    public void DisplayOutline(bool show)
    {
        if (!CanInteract()) return;
        if (show && GameManager.Instance.Camera.IsTDVCurrent)
        {
            StartPickupSequence(true);
            return;
        }

        if (_outline != null)
        {
            _outline.ToggleOutline(show);
        }
    }
}
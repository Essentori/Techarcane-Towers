using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BuildingModeController : MonoBehaviour
{
    private enum ConstructionType
    {
        Tower,
        Village
    }
    private enum BuildPhase 
    { 
        None, 
        PlanningPlacement,
        ConstructionPlacement,
        ConstructionRotate,
        ConfirmPlacement 
    }

    private GameManager _hub;
    private PlayerInputHandler _input;

    private GameObject _constructionBlueprintObject;
    private BlueprintStateController _currentConstruction;

    private ConstructionType _constructionType;

    [Header("Placement Validation Settings")]
    [SerializeField] private GameObject _placementProjectionPrefab;
    [SerializeField] private float _constructionRadius = 2.5f;
    [SerializeField] private float _maxSlopeAngle = 15f;
    [SerializeField] private float _maxElevationDifference = 0.25f;
    private Vector3 _projectionRingSize => new Vector3(_constructionRadius*2f, _constructionRadius*2f, 30f);

    [Header("Player Out-of-area Pusher Settings")]
    [SerializeField] private float _pushDistanceMargin = 1f;
    [SerializeField] private float _pushDuration = 0.7f;
    [SerializeField] private float _pushHeight = 1.5f;

    private BuildPhase _currentBuildPhase = BuildPhase.None;
    private GameObject _visualRingObject;
    private DecalProjector _decalProjector; //of _visualRingObject
    private Material _decalMaterialInstance;
    private Material _blueprintMaterialInstance;

    private Vector3 _targetPlacementPos;
    private float _blueprintRotationY = 0f;
    private bool _isPlacementValid = false;

    private int _snapCycleIndex = 0;
    private float _currentGridSize = 0f;
    private bool _isGridSnappingActive => _currentGridSize > 0;

    void Start()
    {
        _hub = GameManager.Instance;
        _input = _hub.Player.InputHandler;
        _input.OnBuildingModePerformed += OnToggleBuildingMode;
        _input.OnGridSnappingPerformed += OnToggleGridSnapping;
        _input.OnRotateTowerPerformed += OnRotateTowerPerformed;
        _constructionType = ConstructionType.Tower;
    }

    void OnDestroy()
    {
        ExitBuildingMode();
        _input.OnBuildingModePerformed -= OnToggleBuildingMode;
        _input.OnGridSnappingPerformed -= OnToggleGridSnapping;
        _input.OnRotateTowerPerformed -= OnRotateTowerPerformed;
    }

    private void OnToggleBuildingMode()
    {
        if (_currentBuildPhase != BuildPhase.None) ExitBuildingMode();
        else EnterBuildingMode();
    }

    private void EnterBuildingMode()
    {
        _hub.Player.State.ApplyState(PlayerState.InBuildingMode);
        ChangeBuildingState(BuildPhase.PlanningPlacement);

        _visualRingObject = Instantiate(_placementProjectionPrefab);
        _decalProjector = _visualRingObject.GetComponent<DecalProjector>();
        _decalProjector.transform.localScale = _projectionRingSize;

        _constructionBlueprintObject = null;
        _currentConstruction = null;
    }

    private void ExitBuildingMode()
    {
        _hub.Player.State.RemoveState(PlayerState.InBuildingMode);
        ChangeBuildingState(BuildPhase.None);

        Destroy(_visualRingObject);
        Destroy(_decalMaterialInstance);
        if (_constructionBlueprintObject) Destroy(_constructionBlueprintObject);
        if (_blueprintMaterialInstance) Destroy(_blueprintMaterialInstance);
    }

    private void ChangeBuildingState(BuildPhase phase)
    {
        _currentBuildPhase = phase;
    }

    private void OnToggleGridSnapping()
    {
        if (_currentBuildPhase == BuildPhase.None) return;
        _snapCycleIndex++;
        _currentGridSize = _snapCycleIndex switch
        {
            1 => 2.0f,
            2 => 1.0f,
            3 => 0.5f,
            _ => 0f
        };
        if (_currentGridSize == 0f) _snapCycleIndex = 0;
    }

    private void OnRotateTowerPerformed()
    {
        if (_constructionBlueprintObject == null) return;
        if (_currentBuildPhase == BuildPhase.ConfirmPlacement || _currentBuildPhase == BuildPhase.ConstructionPlacement)
        {
            _blueprintRotationY = (_blueprintRotationY + 45f) % 360f;
            _constructionBlueprintObject.transform.rotation = Quaternion.Euler(0f, _blueprintRotationY, 0f);
        }
    }

    public void OnConstructionSelected(GameObject prefab)
    {
        if (prefab == null || prefab.name == _constructionBlueprintObject.name) return;
        Destroy(_constructionBlueprintObject);
        _constructionBlueprintObject = Instantiate(prefab); ;

        _currentConstruction = _constructionBlueprintObject.GetComponent<BlueprintStateController>();
        _blueprintMaterialInstance = Instantiate(_hub.Materials.BlueprintTower);
        _currentConstruction.Initialize(_blueprintMaterialInstance, out _constructionRadius);

        _constructionBlueprintObject = ((Component)_currentConstruction).gameObject;
        _constructionBlueprintObject.transform.rotation = Quaternion.Euler(0f, _blueprintRotationY, 0f);
        ChangeBuildingState(BuildPhase.ConstructionPlacement);
        UpdateVisuals();
    }

    void Update()
    {
        switch (_currentBuildPhase)
        {
            case BuildPhase.None: return;
            case BuildPhase.PlanningPlacement: PositionChangeHandle(false); return;
            case BuildPhase.ConfirmPlacement: ConfirmPlacementHandle(); return;
            default: break;
        }
    }
    private void PositionChangeHandle(bool withConstruction)
    {
        UpdatePlacementPosition();
        ValidatePlacement();
        ValidateCollision();
        UpdateVisuals();
        if (withConstruction) AlignConstruction();

        if (_input.AttackTriggered && _isPlacementValid)
        {
            TransitionToConfirmPlacement();
        }
    }
    private void ConfirmPlacementHandle()
    {
        if (_input.AltAttackTriggered)
        {
            CancelConfirmPhase();
            return;
        }
        if (_input.AltAttackTriggered)
        {
            FinalizeBlueprintPlacement();
        }
    }

    private void UpdatePlacementPosition()
    {
        CamState cameraState = _hub.Camera.CurrentState;
        float raycastDistance = _hub.Player.State.IsTDV ? 2000f : 50f;

        Ray ray = _hub.Camera.GetCurrentLookRay();

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, _hub.Layers.Buildibale))
        {
            _targetPlacementPos = hit.point;

            if (_isGridSnappingActive)
            {
                _targetPlacementPos.x = Mathf.Round(_targetPlacementPos.x / _currentGridSize) * _currentGridSize;
                _targetPlacementPos.z = Mathf.Round(_targetPlacementPos.z / _currentGridSize) * _currentGridSize;
            }
        }
    }

    private void ValidatePlacement()
    {
        #region Position Validation
        Vector3 center = _targetPlacementPos;
        Vector3[] checkPoints = new Vector3[9];
        checkPoints[0] = center;
        int pointsAmount = checkPoints.Length;
        float pointsDegreeDifference = 360f / (pointsAmount - 1);

        for (int i = 0; i < pointsAmount - 1; i++)
        {
            float angle = i * (pointsDegreeDifference) * Mathf.Deg2Rad;
            checkPoints[i + 1] = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _constructionRadius;
        }

        _isPlacementValid = true;
        float centerHitY = center.y;

        for (int i = 0; i < pointsAmount; i++)
        {
            bool hitsGround = Physics.Raycast(checkPoints[i] + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, _hub.Layers.Buildibale);

            if (!hitsGround)
            {
                _isPlacementValid = false;
                return;
            }

            if (i == 0) centerHitY = groundHit.point.y;

            float slopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            float elevationDifference = Mathf.Abs(groundHit.point.y - centerHitY);
            _isPlacementValid =
                slopeAngle <= _maxSlopeAngle &&
                elevationDifference <= _maxElevationDifference;
            if (!_isPlacementValid) return;
        }
        #endregion
    }

    private void ValidateCollision()
    {
        #region Collision Validation
        Vector3 center = _targetPlacementPos;
        Collider[] nearbyObstacles = Physics.OverlapSphere(center + Vector3.up, _constructionRadius * 2f, _hub.Layers.ObstaclesForBuilding);
        Vector2 center2D = new Vector2(center.x, center.z);

        foreach (Collider collider in nearbyObstacles)
        {
            Vector3 closestPoint;
            if (collider is MeshCollider meshCol && !meshCol.convex)
            {
                closestPoint = collider.bounds.ClosestPoint(center + Vector3.up * 1.0f);
            }
            else
            {
                closestPoint = collider.ClosestPoint(center + Vector3.up * 1.0f);
            }

            Vector2 closestPoint2D = new Vector2(closestPoint.x, closestPoint.z);

            float distanceXZ = Vector2.Distance(center2D, closestPoint2D);

            if (distanceXZ < _constructionRadius)
            {
                _isPlacementValid = false;
                return;
            }
        }
        #endregion
    }


    private void UpdateVisuals()
    {
        _visualRingObject.transform.position = _targetPlacementPos + Vector3.up * 0.1f;
        _visualRingObject.transform.localScale = _projectionRingSize;
        _decalMaterialInstance.SetFloat("_IsValid", _isPlacementValid ? 1f : 0f);
        if (_currentConstruction != null)
        {
            _currentConstruction.SetVisuals(isValid: _isPlacementValid);
        }
    }

    private void OpenCurrentTypeMenu()
    {
        switch (_constructionType)
        {
            case ConstructionType.Tower:
            {
                _hub.Menus.TowerSelectMenu.OpenMenu(out _constructionBlueprintObject);
                return;
            }
            case ConstructionType.Village:
            {
                return;
            }
        }
        OnConstructionSelected(_constructionBlueprintObject);
    }

    private void TransitionToConfirmPlacement()
    {
        ChangeBuildingState(BuildPhase.ConfirmPlacement);
        if (_constructionBlueprintObject == null) OpenCurrentTypeMenu();
        _currentConstruction.SetVisuals(isBlueprint: true);
        AlignConstruction();
    }

    private void CancelConfirmPhase()
    {
        ChangeBuildingState(BuildPhase.ConstructionPlacement);
    }

    private void FinalizeBlueprintPlacement()
    {
        AlignConstruction();
        if (IsPlayerOverlapping(_constructionBlueprintObject))
        {
            StartCoroutine(PushPlayerOut(_constructionBlueprintObject.transform.position));
        }
        ExitBuildingMode();
    }

    private void AlignConstruction()
    {
        if (_constructionBlueprintObject == null) OpenCurrentTypeMenu();
        _constructionBlueprintObject.transform.position = _targetPlacementPos;
        Physics.SyncTransforms();
        Collider baseCollider = _currentConstruction.BaseCollider;
        float bottomYOffset = _constructionBlueprintObject.transform.position.y - baseCollider.bounds.min.y;
        _constructionBlueprintObject.transform.position += Vector3.up * bottomYOffset;
        Physics.SyncTransforms();
    }

    private bool IsPlayerOverlapping(GameObject constructionToCheck)
    {
        float distance = Vector3.Distance(_hub.Player.transform.position, constructionToCheck.transform.position);
        return distance < _constructionRadius;
    }

    private IEnumerator PushPlayerOut(Vector3 towerPosition)
    {
        _hub.Player.State.SetMoveLock(true, "PlayerPush");
        _hub.Player.Movement.ResetVerticalVelocity();

        Vector3 pushDirection = (_hub.Player.transform.position - towerPosition);
        pushDirection.y = 0;

        if (pushDirection.sqrMagnitude < 0.01f) pushDirection = -_hub.Player.transform.forward;
        pushDirection.Normalize();

        float puchDistance = _constructionRadius + _pushDistanceMargin;

        Vector3 startPlayerPos = _hub.Player.transform.position;
        Vector3 endPlayerPos = startPlayerPos + pushDirection * puchDistance + Vector3.up * _pushHeight;

        float elapsed = 0f;
        while (elapsed < _pushDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _pushDuration;
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);

            Vector3 currentPos = Vector3.Lerp(startPlayerPos, endPlayerPos, easedT);

            _hub.Player.Movement.Controller.enabled = false;
            _hub.Player.transform.position = currentPos;
            _hub.Player.Movement.Controller.enabled = true;

            yield return null;
        }

        _hub.Player.State.SetMoveLock(false, "PlayerPush");
    }

}
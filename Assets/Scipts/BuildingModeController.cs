using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BuildingModeController : MonoBehaviour
{
    private enum BuildPhase
    {
        None,
        MenuSelection,
        Placement,
    }

    [Header("Placement Validation Settings")]
    [SerializeField] private GameObject _placementDecalPrefab;
    [SerializeField] private float _maxSlopeAngle = 15f;
    [SerializeField] private float _maxElevationDifference = 0.25f;

    private Vector3 _placementDecalSize => new Vector3(_currentRadius * 2f, _currentRadius * 2f, 30f);
    private float _currentRadius = 1f;

    private BuildPhase _currentBuildPhase = BuildPhase.None;
    private Vector3 _targetPlacementPos;
    private float _contstructionRotationY = 0f;
    private bool _isPlacementValid = false;

    private GameManager _hub;
    private LayersHandler _layersRef;
    private PlayerController _player;
    private UI_ConstructionMenu _menu;

    private GameObject _constructionObject;
    private BlueprintStateController _constructionController;
    private Material _blueprintMaterialInstance;
    private GameObject _decalRadiusValidator;
    private DecalProjector _validatorDecalProjector;


    private int _snapCycleIndex = 0;
    private float _currentGridSize = 0f;
    private bool _isGridSnappingActive => _currentGridSize > 0;

    private Collider[] _obstacleCollidersBuffer = new Collider[32];
    private bool _skipThisFrameChecks = false;

    void Start()
    {
        _hub = GameManager.Instance;
        _layersRef = _hub.Layers;
        _player = _hub.Player;
        _menu = _hub.Menus.ConstructionPick;
        PlayerInputHandler input = _player.InputHandler;
        input.OnBuildingModePerformed += HandleBuildingModePressed;
        input.OnGridSnappingPerformed += HandleGridSnappingPressed;
        input.OnRotateTowerPerformed += HandleConstructionRotate;
        input.OnAttackPerformed += HandleAttackPressed;
        input.OnAltAttackPerformed += HandleAltAttackPressed;

        _menu.OnConstructionConfirmed += OnConstructionSelected;
    }

    void OnDestroy()
    {
        if(_hub != null)
        {
            PlayerInputHandler input = _player.InputHandler;
            input.OnBuildingModePerformed -= HandleBuildingModePressed;
            input.OnGridSnappingPerformed -= HandleGridSnappingPressed;
            input.OnRotateTowerPerformed -= HandleConstructionRotate;
            input.OnAttackPerformed -= HandleAttackPressed;
            input.OnAltAttackPerformed -= HandleAltAttackPressed;

            _menu.OnConstructionConfirmed -= OnConstructionSelected;
        }
    }

    private void HandleBuildingModePressed()
    {
        if (_currentBuildPhase != BuildPhase.None) ExitBuildingMode();
        else EnterBuildingMode();
    }

    private void EnterBuildingMode()
    {
        _hub.Player.State.SetState(PlayerState.InBuildingMode, true);
        _currentBuildPhase = BuildPhase.MenuSelection;

        _menu.OpenMenu();
    }

    private void ExitBuildingMode()
    {
        if (_menu.enabled == true) _menu.CloseMenu();

        ClearInstances();

        _currentBuildPhase = BuildPhase.None;
        _hub.Player.State.SetState(PlayerState.InBuildingMode, false);
    }
    private void ClearInstances()
    {

        if (_constructionObject != null) Destroy(_constructionObject);
        if (_decalRadiusValidator != null) Destroy(_decalRadiusValidator);

        _constructionObject = null;
        _constructionController = null;
        if (_blueprintMaterialInstance != null) Destroy(_blueprintMaterialInstance);
    }

    private void Update()
    {
        if (_validatorDecalProjector != null) _validatorDecalProjector.material.SetFloat("_Unscaled_Time", Time.unscaledTime);
        if (_currentBuildPhase != BuildPhase.None && _constructionObject != null)
        {
            UpdatePlacementPosition();
            Calculate();
        }
    }

    private void Calculate()
    {
        if (!_skipThisFrameChecks)
        {
            AlignConstruction();
            ValidatePlacement();
        }
        ValidateCollision();
        UpdateVisuals();
    }

    private void HandleAttackPressed()
    {
        switch (_currentBuildPhase)
        {
            case BuildPhase.None:
                return;
            case BuildPhase.MenuSelection:
                break;
            case BuildPhase.Placement:
                PlaceBlueprint();
                break;
        }
    }

    private void HandleAltAttackPressed()
    {
        switch (_currentBuildPhase)
        {
            case BuildPhase.None:
                return;
            case BuildPhase.MenuSelection:
                ExitBuildingMode();
                break;
            case BuildPhase.Placement:
                ReturnToMenuSelection();
                break;
        }
    }
    private void HandleGridSnappingPressed()
    {
        if (_currentBuildPhase == BuildPhase.None) return;
        _snapCycleIndex++;
        _currentGridSize = _snapCycleIndex switch
        {
            1 => 1.0f,
            2 => 0.5f,
            3 => 0.25f,
            _ => 0f
        };
        if (_currentGridSize == 0f) _snapCycleIndex = 0;
    }
    private void HandleConstructionRotate()
    {
        if (_constructionObject == null || _currentBuildPhase == BuildPhase.None) return;
        _contstructionRotationY = (_contstructionRotationY + 45f) % 360f;
        _constructionObject.transform.rotation = Quaternion.Euler(0f, _contstructionRotationY, 0f);
    }

    public void OnConstructionSelected(GameObject prefab, ConstructionType type)
    {
        _currentBuildPhase = BuildPhase.Placement;

        _constructionObject = Instantiate(prefab);
        _constructionController = _constructionObject.GetComponent<BlueprintStateController>();
        _blueprintMaterialInstance = Instantiate(GameManager.Instance.Materials.BlueprintMaterial);

        if (_constructionController != null)
        {
            _constructionController.Initialize(_blueprintMaterialInstance, out float radius);
            _currentRadius = radius;
        }

        if (_placementDecalPrefab != null)
        {
            _decalRadiusValidator = Instantiate(_placementDecalPrefab);

            _validatorDecalProjector = _decalRadiusValidator.GetComponent<DecalProjector>();
            _validatorDecalProjector.transform.localScale = _placementDecalSize;
        }

        Calculate();
    }

    private void UpdatePlacementPosition()
    {
        // TODO: Fixed position slots for Village construction
        float raycastDistance = _player.State.IsTDV ? 1000f : 50f;
        Ray ray = _hub.Camera.GetCurrentLookRay();

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, _layersRef.Buildable))
        {
            if (Vector3.SqrMagnitude(_targetPlacementPos - hit.point) < 0.0001f)
            {
                _skipThisFrameChecks = true;
                return;
            }

            _skipThisFrameChecks = false;
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
        Vector3 center = _targetPlacementPos;
        _isPlacementValid = true;
        float centerHitY = center.y;

        int pointsAmount = 8;
        float pointsDegreeDifference = 360f / (pointsAmount);
        pointsAmount++;
        for (int i = 0; i < pointsAmount; i++)
        {
            Vector3 currentCheckPoint = center;

            if (i > 0)
            {
                float angle = (i - 1) * pointsDegreeDifference * Mathf.Deg2Rad;
                currentCheckPoint += new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _currentRadius;
            }

            bool hitsGround = Physics.Raycast(
                currentCheckPoint + Vector3.up * 5f,
                Vector3.down, out RaycastHit groundHit,
                10f, _hub.Layers.Buildable);

            if (!hitsGround)
            {
                _isPlacementValid = false;
                return;
            }

            if (i == 0) centerHitY = groundHit.point.y;

            float slopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            float elevationDifference = Mathf.Abs(groundHit.point.y - centerHitY);

            if (slopeAngle > _maxSlopeAngle || elevationDifference > _maxElevationDifference)
            {
                _isPlacementValid = false;
                return;
            }
        }
    }

    private void ValidateCollision()
    {
        if (!_isPlacementValid) return;

        Vector3 center = _targetPlacementPos;
        int hitCount = Physics.OverlapSphereNonAlloc(center + Vector3.up, _currentRadius * 2f, _obstacleCollidersBuffer, _layersRef.ObstaclesForBuilding);
        Vector2 center2D = new Vector2(center.x, center.z);

        for (int i = 0; i < hitCount; i++)
        {
            Collider obstacleCollider = _obstacleCollidersBuffer[i];
            Vector3 closestPoint;

            if (obstacleCollider is MeshCollider meshCol && !meshCol.convex)
            {
                closestPoint = obstacleCollider.bounds.ClosestPoint(center + Vector3.up * 1.0f);
            }
            else
            {
                closestPoint = obstacleCollider.ClosestPoint(center + Vector3.up * 1.0f);
            }

            Vector2 closestPoint2D = new Vector2(closestPoint.x, closestPoint.z);
            float distanceXZ = Vector2.Distance(center2D, closestPoint2D);

            if (distanceXZ < _currentRadius)
            {
                _isPlacementValid = false;
                return;
            }
        }
    }
    private void UpdateVisuals()
    {
        if (_decalRadiusValidator != null)
        {
            _decalRadiusValidator.transform.position = _targetPlacementPos + Vector3.up * 0.1f;
            _decalRadiusValidator.transform.localScale = _placementDecalSize;
            _validatorDecalProjector.material.SetFloat("_IsValid", _isPlacementValid ? 1f : 0f);
        }

        if (_constructionController != null)
        {
            _constructionController.SetPlacementState(isValid: _isPlacementValid);
        }
    }
    private void AlignConstruction()
    {
        if (_constructionObject == null) return;
        _constructionObject.transform.position = _targetPlacementPos;
        Physics.SyncTransforms();

        float bottomYOffset = _constructionObject.transform.position.y - _constructionController.Bottom.y;
        _constructionObject.transform.position += Vector3.up * bottomYOffset;
        Physics.SyncTransforms();
    }

    private void PlaceBlueprint()
    {
        if (!_isPlacementValid) return;

        if (_constructionController != null)
        {
            _constructionController.SetupAsPlacedBlueprint();
        }

        _constructionObject = null;
        _constructionController = null;
        ExitBuildingMode();
    }

    private void ReturnToMenuSelection()
    {
        BlueprintStateController savedPrefab = _constructionController;
        ConstructionType savedType = savedPrefab.Type;

        ClearInstances();
        _currentBuildPhase = BuildPhase.MenuSelection;
        _menu.OpenMenu();
        _menu.SwitchToTab(savedType);
    }
}
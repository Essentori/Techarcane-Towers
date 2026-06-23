using System.Collections.Generic;
using UnityEngine;

public class BlueprintStateController : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public ConstructionType Type;
    public int ManaCost;
    public ResourceRequirement[] ResourceCosts;
    [SerializeField] private float _radiusMargin = 0.25f;

    [Header("Player Out-of-area Push Settings")]
    [SerializeField] private float _pushDistanceMargin = 2f;
    // TODO: Smooth animated player push
    //[SerializeField] private float _pushDuration = 0.7f;
    //[SerializeField] private float _pushHeight = 1.5f;

    private bool _isValidToPlace = true;
    private bool _isPlaced = false;
    private List<RendererData> _partsDataCache = new List<RendererData>();
    private GameManager _hub => GameManager.Instance;
    private Material _blueprintMaterial;
    private LayersHandler _layersRef;
    private IBuildable _mainComponent;
    private string _finalName;
    private bool _isComplete = false;
    private InteractableOutline _outlineController;
    public Vector3 Bottom => _mainComponent.Bottom;

    public void Initialize(Material blueprintMaterialInstance, out float radius)
    {
        _blueprintMaterial = new Material(blueprintMaterialInstance);
        _mainComponent = GetComponent<IBuildable>();
        _outlineController = GetComponent<InteractableOutline>();
        MonoBehaviour mainComponent = _mainComponent as MonoBehaviour;
        if(mainComponent.enabled) mainComponent.enabled = false;
        _layersRef = GameManager.Instance.Layers;

        CacheRendererData();
        SetupAsMovableBlueprint();
        SetAllPartsIgnoreDecal(true);
        ((IBuildable)_mainComponent).CalculateRadius();
        radius = _mainComponent.ConstructionRadius + _radiusMargin; ;
    }

    private void CacheRendererData()
    {
        _partsDataCache.Clear();
        foreach (var part in GetComponentsInChildren<Renderer>())
        {
            _partsDataCache.Add(new RendererData(part, part.sharedMaterials));
        }
    }

    public void SetupAsMovableBlueprint()
    {
        SetAllPartsLayer(_layersRef.IgnoreRaycast);

        foreach (var part in _partsDataCache)
        {
            Material[] blueprintMaterials = new Material[part.OriginalMaterials.Length];
            for (int i = 0; i < blueprintMaterials.Length; i++) blueprintMaterials[i] = _blueprintMaterial;
            part.Renderer.sharedMaterials = blueprintMaterials;
        }
        SetPlacementState(isBlueprint: false);
    }

    public void SetupAsPlacedBlueprint()
    {
        SetAllPartsLayer(_layersRef.BlueprintConstruction);
        SetPlacementState(isBlueprint: true);
        SetAllPartsIgnoreDecal(false);
        _finalName = GameManager.Instance.NameRandomizer.RandomizeConstructionName(_mainComponent.Name);
    }

    private void SetAllPartsLayer(int newLayer)
    {
        foreach (var part in _partsDataCache) part.Renderer.gameObject.layer = newLayer;
    }

    public void SetPlacementState(bool? isValid = null, bool? isBlueprint = null)
    {
        _isValidToPlace = isValid ?? _isValidToPlace;
        _isPlaced = isBlueprint ?? _isPlaced;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_blueprintMaterial == null || _partsDataCache == null) return;
        _blueprintMaterial.SetFloat("_IsValid", _isValidToPlace ? 1f : 0f);
        _blueprintMaterial.SetFloat("_IsBlueprint", _isPlaced ? 1f : 0f);
    }

    private void SetAllPartsIgnoreDecal(bool ignoreDecalLayer)
    {
        foreach (var r in _partsDataCache)
        {
            r.Renderer.renderingLayerMask = ignoreDecalLayer ? _layersRef.IgnoreDecal : _layersRef.DefaultRenderingLayer;
        }
    }

    public bool CanInteract() => enabled && _isPlaced && !_isComplete;

    public string GetInteractPrompt()
    {
        string interactKey = GameManager.Instance.GetInteractKeyName();
        return $"Press {interactKey} to view the {_finalName} construction process";
    }

    public void Interact()
    {
        CompleteBuilding();
    }

    public void DisplayOutline(bool show)
    {
        if (_isComplete) return;
        _outlineController.ToggleOutline(show);
    }
    public void CompleteBuilding()
    {
        _isComplete = true;
        foreach (var part in _partsDataCache)
        {
            if (part.Renderer != null)
            {
                part.Renderer.sharedMaterials = part.OriginalMaterials;
                part.Renderer.renderingLayerMask = _layersRef.DefaultRenderingLayer;
            }
        }
        SetAllPartsIgnoreDecal(false);
        SetAllPartsLayer(_layersRef.GetLayerByType(Type));
        if (CheckPlayerOverlapping())
        {
            PushPlayerOut();
        } 
        SetAllPartsLayer(_layersRef.GetLayerByType(Type));
        _mainComponent.Initialize(_finalName);
        Destroy(this);
    }
    private bool CheckPlayerOverlapping()
    {
        float distance = Vector3.Distance(_hub.Player.transform.position, Bottom);
        return distance < _mainComponent.ConstructionRadius * 2;
    }

    private void PushPlayerOut()
    {
        Vector3 pushDirection = (_hub.Player.transform.position - Bottom);
        pushDirection.y = 0;

        if (pushDirection.sqrMagnitude < 0.01f)
            pushDirection = -_hub.Player.transform.forward;

        pushDirection.Normalize();

        float pushDistance = _mainComponent.ConstructionRadius + _pushDistanceMargin;
        Vector3 finalTargetPos = FindSafePushPosition(pushDirection, pushDistance);
        if (finalTargetPos == Vector3.zero)
        {
            finalTargetPos = _hub.PlayerSpawnPoint.position;
        }
        _hub.Player.Movement.Controller.enabled = false;
        _hub.Player.transform.position = finalTargetPos;
        Physics.SyncTransforms();
        _hub.Player.Movement.Controller.enabled = true;
    }
    private static int _checkRotationIterations = 8;
    private Vector3 FindSafePushPosition(Vector3 direction, float distance)
    {
        float angle = Mathf.Round(360f / _checkRotationIterations);
        for (int i = 0; i < _checkRotationIterations; i++)
        {
            Vector3 rotatedDirection = Quaternion.Euler(0f, angle * i, 0f) * direction;
            Vector3 checkPos = Bottom + rotatedDirection * distance;
            if (TryGetSafeGroundPosition(checkPos, out Vector3 safeGroundPos)) return safeGroundPos;
        }
        return Vector3.zero;
    }

    private bool TryGetSafeGroundPosition(Vector3 checkPos, out Vector3 safePos)
    {
        safePos = checkPos;
        var controller = _hub.Player.Movement.Controller;
        float playerHeight = controller.height;
        float playerRadius = controller.radius;

        Vector3 rayStart = checkPos + Vector3.up * 5f;
        if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit groundHit, 15f, _layersRef.Walkable))
        {
            return false;
        }
        safePos = groundHit.point;
        Vector3 capsuleBottom = safePos + Vector3.up * playerRadius;
        Vector3 capsuleTop = safePos + Vector3.up * (playerHeight - playerRadius);
        bool isObstructed = Physics.CheckCapsule(
            capsuleBottom,
            capsuleTop,
            playerRadius,
            _layersRef.ObstaclesForBuilding
        );
        return !isObstructed;
    }
}
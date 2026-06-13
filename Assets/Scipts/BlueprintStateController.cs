using System.Collections.Generic;
using UnityEngine;

public class BlueprintStateController : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [field: SerializeField] public GameObject ConstructionBase { get; private set; }
    public float ConstructionRadius { get; private set; }
    [SerializeField] private float _radiusMargin = 0.25f;
    [HideInInspector] public Collider BaseCollider { get; private set; }
    [HideInInspector] public float Bottom => BaseCollider != null ? BaseCollider.bounds.min.y : 0f;
    private bool _isValidToPlace = false;
    private bool _isPlaced = false;
    private List<RendererData> _partsDataCashe = new List<RendererData>();
    private Material _blueprintMaterial;
    private LayersHandler _layersRef;
    private IBuildable _mainComponent;
    private string _finalName;

    public void Initialize(Material blueprintMaterialInstance, out float radius)
    {
        _blueprintMaterial = blueprintMaterialInstance;
        _mainComponent = GetComponent<IBuildable>();
        _layersRef = GameManager.Instance.Layers;
        BaseCollider = ConstructionBase.GetComponent<Collider>();
        CacheRendererData();
        CalculateRadius();
        SetupAsMovableBlueprint();
        SetAllPartsIgnoreDecal(true);

        radius = ConstructionRadius;
    }
    private void CacheRendererData()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            _partsDataCashe.Add(new RendererData
            {
                Renderer = r,
                OriginalMaterials = r.sharedMaterials
            });
        }
    }
    private void CalculateRadius()
    {
        Vector3 extents = BaseCollider.bounds.extents;
        float baseHorizontalRadius = Mathf.Max(extents.x, extents.z);

        ConstructionRadius = baseHorizontalRadius + _radiusMargin;
    }
    public void SetupAsMovableBlueprint()
    {
        SetAllPartsLayer(_layersRef.BlueprintTower);

        foreach (var part in _partsDataCashe)
        {
            Material[] blueprintMaterials = new Material[part.OriginalMaterials.Length];
            for (int i = 0; i < blueprintMaterials.Length; i++) blueprintMaterials[i] = _blueprintMaterial;
            part.Renderer.sharedMaterials = blueprintMaterials;
        }
        _isPlaced = false;
        UpdateVisuals();
    }
    public void SetupAsStateBlueprint()
    {
        _isPlaced = true;
        UpdateVisuals();
    }
    private void SetAllPartsLayer(int newLayer)
    {
        GameObject[] allParts = GetComponentsInChildren<GameObject>();
        foreach (GameObject part in allParts)
        {
            part.layer = newLayer;
        }
    }
    private void UpdateVisuals()
    {
        _blueprintMaterial.SetFloat("_IsValidToPlace", _isValidToPlace ? 1f : 0f);
        _blueprintMaterial.SetFloat("_IsBlueprint", _isPlaced ? 1f : 0f);
    }
    public void SetVisuals(bool? isValid = null, bool? isBlueprint = null)
    {
        _isValidToPlace = isValid ?? _isValidToPlace;
        _isPlaced = isBlueprint ?? _isPlaced;
        UpdateVisuals();
    }
    private void SetAllPartsIgnoreDecal(bool ignoreDecalLayer)
    {
        foreach (var part in _partsDataCashe)
        {
            part.Renderer.renderingLayerMask = ignoreDecalLayer ? _layersRef.IgnoreDecal : _layersRef.DefaultRenderingLayer;
        }
    }

    public void CompleteBuilding()
    {
        foreach (var part in _partsDataCashe)
        {
            part.Renderer.materials = part.OriginalMaterials;
            part.Renderer.renderingLayerMask = _layersRef.DefaultRenderingLayer;
        }
        SetAllPartsIgnoreDecal(false);
        ((MonoBehaviour)_mainComponent).enabled = true;
        _mainComponent.Initialize(_partsDataCashe);
        GameManager.Instance.NameRandomizer.RandomizeConstructionName(_mainComponent);

        Destroy(this);
    }
    public bool CanInteract() => enabled;

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
        LayerMask changeToLayer = show ? _layersRef.BluePrintTowerSelected : _layersRef.BlueprintTower;
        SetAllPartsLayer(changeToLayer);
    }
}
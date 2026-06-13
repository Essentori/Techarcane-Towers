using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : MonoBehaviour, IInteractable, IBuildable
{
    protected GameManager _hub;
    protected LayerMask _defaultTowerLayer;
    [SerializeField] protected TowerData _stats;
    protected abstract void ActivateTower();
    protected abstract void OnPowerOn();
    protected abstract void OnPowerOff();

    private List<RendererData> _partsDataCashe = new List<RendererData>();
    private List<GameObject> _towerParts = new List<GameObject>();

    public string Name { get; private set; }
    public void SetName(string newName) => Name = newName;
    public bool IsOperational { get; private set; }

    protected TargetPriority _targetPriority;
    void Awake()
    {
        _hub = GameManager.Instance;
        _defaultTowerLayer = _hub.Layers.Tower;
    }
    public void Initialize(List<RendererData> data)
    {
        _partsDataCashe = data;
        _towerParts.Add(gameObject);
        _towerParts.AddRange(GetComponentsInChildren<GameObject>());
        IsOperational = true;
        OnPowerOn();
        ActivateTower();
    }
    public void SwitchPowerState()
    {
        IsOperational = !IsOperational;
        if (IsOperational) OnPowerOn();
        else OnPowerOff();
    }
    public bool CanInteract() => enabled;

    public string GetInteractPrompt()
    {
        return $"Press {_hub.GetInteractKeyName()} to configure the tower ({Name})";
    }

    public virtual void Interact() => _hub.Menus.TowerMenu.OpenMenu(this);
    public void DisplayOutline(bool show)
    {
        foreach (var part in _partsDataCashe)
        {

            if (show)
            {
                Material[] baseMaterials = part.OriginalMaterials;
                List<Material> combinedMats = new List<Material>(baseMaterials ?? part.OriginalMaterials);
                combinedMats.Add(_hub.Materials.InteractibleOutline);
                part.Renderer.sharedMaterials = combinedMats.ToArray();
            }
            else part.Renderer.materials = part.OriginalMaterials;
        }
    }
}
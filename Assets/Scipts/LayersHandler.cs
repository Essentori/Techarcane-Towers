using UnityEngine;

public class LayersHandler : MonoBehaviour
{
    [field: SerializeField] public LayerMask Tower {  get; private set; }
    [field: SerializeField] public LayerMask Enemy { get; private set; }
    [field: SerializeField] public LayerMask Interactible { get; private set; }
    [field: SerializeField] public LayerMask Buildibale { get; private set; }
    [field: SerializeField] public LayerMask ObstaclesForBuilding { get; private set; }
    [field: SerializeField] public LayerMask BlueprintTower { get; private set; }
    [field: SerializeField] public LayerMask BluePrintTowerSelected { get; private set; }
    [field: SerializeField] public RenderingLayerMask IgnoreDecal { get; private set; }
    [field: SerializeField] public RenderingLayerMask DefaultRenderingLayer { get; private set; }
    
}

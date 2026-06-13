using UnityEngine;

public struct RendererData
{
    public MeshRenderer Renderer;
    public Material[] OriginalMaterials;
}
public enum CamState
{
    FirstPersonView,
    TransitionToTopDown,
    TopDownView,
    TransitionToFirstPerson,
    Locked
}
public enum TargetPriority 
{ 
    First, 
    Last, 
    LowestHealth, 
    HighestHealth 
}
public enum PlayerState
{
    Locked,
    FPV,
    TDV,
    Crouching,
    Sprinting,
    Flying,
    CanInteract,
    CanAttack,
    InBuildingMode,
}
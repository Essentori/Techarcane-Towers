using UnityEngine;

#region Player 
public enum CamState
{
    FirstPersonView,
    TransitionToTopDown,
    TopDownView,
    TransitionToFirstPerson,
    Locked
}
public enum PlayerState
{
    Locked,
    Crouching,
    Sprinting,
    Flying,
    InBuildingMode,
}
#endregion
#region Buildings
public struct RendererData
{
    public Renderer Renderer;
    public Material[] OriginalMaterials;

    public RendererData(Renderer r, Material[] materials) : this()
    {
        Renderer = r;
        OriginalMaterials = materials;
    }
}
public enum ConstructionType
{
    Tower,
    Village
}
public enum ResourceType
{
    Wood,
    Stone,
    Iron
}

[System.Serializable]
public struct ResourceRequirement
{
    public ResourceType Type;
    public int Amount;
}
[System.Serializable]
public struct ResourceMapping
{
    public ResourceType Type;
    public string ResourceName;
    public Sprite ResourceSprite;
}
#endregion
#region Tower Logic
public enum TargetPriority 
{ 
    First, 
    Last, 
    LowestCurrentHealth, 
    HighestMaxHealth,
    Nearest,
    Farthest,
    Random
}
[System.Flags]
public enum DamageType
{
    Physical = 0,
    Fire = 1 << 0,
    Ice = 1 << 1
}
public enum AttackType
{
    Projectile,
    Beam,
    AOE
}
public enum ModuleTier 
{ 
    Common = 1, 
    Upgraded = 2, 
    Arcane = 3 
}
public enum ConditionType 
{ 
    EveryXProjectile 
}
public enum ActionType 
{
    FinalDamageMultiplyByY 
};
#endregion
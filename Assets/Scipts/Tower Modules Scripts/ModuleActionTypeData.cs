using UnityEngine;

[CreateAssetMenu(fileName = "New Action Module", menuName = "Modules/Action")]
public class ModuleActionTypeData : ScriptableObject
{
    public string ID;
    public string ModuleName;
    public ModuleTier Tier;
    public ActionType Type;

    [Header("Generation Settings")]
    public float MinValueY;
    public float MaxValueY;

    public int SlotCost = 1;
}
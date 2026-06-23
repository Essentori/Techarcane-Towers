using UnityEngine;

[CreateAssetMenu(fileName = "New Condition Module", menuName = "Modules/Condition")]
public class ModuleConditionTypeData : ScriptableObject
{
    public string ID;
    public string ModuleName;
    public ModuleTier Tier;
    public ConditionType Type;

    [Header("Generation Settings")]
    public float MinValueX;
    public float MaxValueX;

    public int BaseActionSlots = 1;
}
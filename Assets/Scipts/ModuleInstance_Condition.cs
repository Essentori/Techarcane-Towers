using System.Collections.Generic;
using UnityEngine;

public class ModuleInstance_Condition
{
    public ModuleConditionTypeData Base { get; private set; }
    public float RolledValue { get; private set; }
    public List<ModuleInstance_Action> SlottedActions { get; set; } = new List<ModuleInstance_Action>();
    public int ValueCounter;
    public bool IsActive { get; set; } = true;
    public ModuleInstance_Condition(ModuleConditionTypeData module)
    {
        Base = module;
        RolledValue = Random.Range(module.MinValueX, module.MaxValueX);
    }
}


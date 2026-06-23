using UnityEngine;

public class ModuleInstance_Action
{
    public ModuleActionTypeData Base { get; private set; }
    public float RolledValue { get; private set; }
    public ModuleInstance_Action(ModuleActionTypeData module)
    {
        Base = module;
        RolledValue = Random.Range(module.MinValueY, module.MaxValueY);
    }
}
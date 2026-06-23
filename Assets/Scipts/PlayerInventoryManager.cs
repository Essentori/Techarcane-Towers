using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<ModuleInstance_Condition> ConditionModules { get; private set; } = new List<ModuleInstance_Condition>();
    public List<ModuleInstance_Action> ActionModules { get; private set; } = new List<ModuleInstance_Action>();
    //public List<AmplifierInstance> Amplifiers { get; private set; } = new List<AmplifierInstance>();
    //public List<ArtifactInstance> Artifacts { get; private set; } = new List<ArtifactInstance>();

    public event Action OnConditionModulesChanged;
    public event Action OnActionModulesChanged;
    //public event Action OnAmplifiersChanged;
    //public event Action OnArtifactsChanged;

    #region Condition Modules
    public void AddConditionModule(ModuleConditionTypeData typeData)
    {
        ModuleInstance_Condition newModule = new ModuleInstance_Condition(typeData);
        ConditionModules.Add(newModule);

        OnConditionModulesChanged?.Invoke();
    }

    public void RemoveConditionModule(ModuleInstance_Condition instance)
    {
        if (ConditionModules.Remove(instance))
        {
            OnConditionModulesChanged?.Invoke();
        }
    }
    #endregion

    #region Action Modules
    public void AddActionModule(ModuleActionTypeData typeData)
    {
        ModuleInstance_Action newModule = new ModuleInstance_Action(typeData);
        ActionModules.Add(newModule);

        OnActionModulesChanged?.Invoke();
    }

    public void RemoveActionModule(ModuleInstance_Action instance)
    {
        if (ActionModules.Remove(instance))
        {
            OnActionModulesChanged?.Invoke();
        }
    }
    #endregion

    #region Amplifiers
    //public void AddAmplifier(AmplifierTypeData typeData)
    //{
    //    AmplifierInstance newAmplifier = new AmplifierInstance(typeData);
    //    Amplifiers.Add(newAmplifier);

    //    OnAmplifiersChanged?.Invoke();
    //}

    //public void RemoveAmplifier(AmplifierInstance instance)
    //{
    //    if (Amplifiers.Remove(instance))
    //    {
    //        OnAmplifiersChanged?.Invoke();
    //    }
    //}
    #endregion

    #region Artifacts
    //public void AddArtifact(ArtifactTypeData typeData)
    //{
    //    ArtifactInstance newArtifact = new ArtifactInstance(typeData);
    //    Artifacts.Add(newArtifact);

    //    OnArtifactsChanged?.Invoke();
    //}

    //public void RemoveArtifact(ArtifactInstance instance)
    //{
    //    if (Artifacts.Remove(instance))
    //    {
    //        OnArtifactsChanged?.Invoke();
    //    }
    //}
    #endregion
}

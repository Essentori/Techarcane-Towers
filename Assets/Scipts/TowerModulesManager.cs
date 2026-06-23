using UnityEngine;
using System.Collections.Generic;

public class TowerModulesManager : MonoBehaviour
{
    private Tower _tower;

    private List<ModuleInstance_Condition> _allConditions = new List<ModuleInstance_Condition>();

    private List<ModuleInstance_Condition> _activeProjectileConditions = new List<ModuleInstance_Condition>();
    //private List<ModuleInstance_Condition> _activeBeamConditions = new List<ModuleInstance_Condition>();
    //private List<ModuleInstance_Condition> _activeAOEConditions = new List<ModuleInstance_Condition>();
    //private List<ModuleInstance_Condition> _activeJumpConditions = new List<ModuleInstance_Condition>();

    private void Awake() => _tower = GetComponent<Tower>();

    [Header("Test Initial Modules")]
    [SerializeField] private ModuleConditionTypeData _testConditionBlueprint;
    [SerializeField] private ModuleActionTypeData _testActionBlueprint;
    private void Start()
    {
        if (_testConditionBlueprint != null && _testActionBlueprint != null)
        {
            ModuleInstance_Condition testCondition = new ModuleInstance_Condition(_testConditionBlueprint);
            ModuleInstance_Action testAction = new ModuleInstance_Action(_testActionBlueprint);
            testCondition.SlottedActions.Add(testAction);

            EquipConditionModule(testCondition);

        }
    }

    public void EquipConditionModule(ModuleInstance_Condition condition)
    {
        if (!_allConditions.Contains(condition))
        {
            _allConditions.Add(condition);
        }
    }

    private void OnEnable()
    {
        if (_tower != null)
        {
            _tower.OnAttackExecuted += HandleTowerAttack;
            _tower.OnTowerStatsRecalculated += RefreshModulesOperationalState;
        }
        // GlobalPlayerEvents.OnPlayerJumped += HandlePlayerJumped;

        RefreshModulesOperationalState();
    }

    private void OnDisable()
    {
        if (_tower != null)
        {
            _tower.OnAttackExecuted -= HandleTowerAttack;
            _tower.OnTowerStatsRecalculated -= RefreshModulesOperationalState;
        }
        // GlobalPlayerEvents.OnPlayerJumped -= HandlePlayerJumped;
    }

    public void RefreshModulesOperationalState()
    {
        _activeProjectileConditions.Clear();
        //_activeBeamConditions.Clear();
        //_activeAOEConditions.Clear();
        //_activeJumpConditions.Clear();

        if (_tower == null) return;

        foreach (var condition in _allConditions)
        {
            if (ModuleValidator.IsCompatible(condition.Base.Type, _tower))
            {
                condition.IsActive = true;

                switch (condition.Base.Type)
                {
                    case ConditionType.EveryXProjectile:
                        _activeProjectileConditions.Add(condition);
                        break;

                    // case ConditionType.OnBeamTick:
                        // _activeBeamConditions.Add(condition);
                        // break;

                    // case ConditionType.OnPlayerJump:
                        // _activeJumpConditions.Add(condition);
                        // break;
                }
            }
            else
            {
                condition.IsActive = false;
            }
        }
    }
    public event System.Action OnModuleActionExecuted;
    private void HandleTowerAttack(AttackType firedType, Projectile projectile, ref ProjectileData data)
    {
        switch (firedType)
        {
            case AttackType.Projectile:
                ExecuteConditionList(_activeProjectileConditions, projectile, ref data);
                break;

            //case AttackType.Beam:
            //    ExecuteConditionList(_activeBeamConditions, projectile, ref data);
            //    break;

            //case AttackType.AOE:
            //    ExecuteConditionList(_activeAOEConditions, projectile, ref data);
            //    break;
        }
    }

    private void ExecuteConditionList(List<ModuleInstance_Condition> list, Projectile projectile, ref ProjectileData data)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var condition = list[i];

            if (ModuleExecutor.TryTriggerCondition(condition))
            {
                for (int j = 0; j < condition.SlottedActions.Count; j++)
                {
                    var action = condition.SlottedActions[j];
                    ModuleExecutor.ExecuteAction(action.Base.Type, projectile, ref data, action.RolledValue);
                    OnModuleActionExecuted?.Invoke();
                }
            }
        }
    }
}
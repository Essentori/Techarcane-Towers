using UnityEngine;

public static class ModuleExecutor
{
    public static bool TryTriggerCondition(ModuleInstance_Condition condition)
    {
        switch (condition.Base.Type)
        {
            case ConditionType.EveryXProjectile:
                condition.ValueCounter++;
                int targetProjectiles = Mathf.RoundToInt(condition.RolledValue);

                if (condition.ValueCounter >= targetProjectiles)
                {
                    condition.ValueCounter = 0;
                    return true;
                }
                break;
        }
        return false;
    }

    public static void ExecuteAction(ActionType type, Projectile projectile, ref ProjectileData data, float rolledPower)
    {
        switch (type)
        {
            case ActionType.FinalDamageMultiplyByY:
                data.ModifiedDamage *= rolledPower;
                break;
        }
    }
}
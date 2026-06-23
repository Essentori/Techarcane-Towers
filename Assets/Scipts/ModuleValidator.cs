public static class ModuleValidator
{
    public static bool IsCompatible(ConditionType conditionType, Tower appliedTower)
    {
        return conditionType switch
        {
            ConditionType.EveryXProjectile => appliedTower.CurrentAttackType == AttackType.Projectile,
            _ => true
        };
    }

    public static bool IsCompatible(ActionType actionType, AttackType towerAttackType)
    {
        return actionType switch
        {
            _ => true
        };
    }
}
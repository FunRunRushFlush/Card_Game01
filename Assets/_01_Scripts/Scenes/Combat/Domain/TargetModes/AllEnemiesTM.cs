using System.Collections.Generic;

public class AllEnemiesTM : TargetMode
{
    public override List<CombatantId> GetTargetIds()
    {
        var enemyIds = EnemySystem.Instance != null ? EnemySystem.Instance.EnemyIds : null;
        if (enemyIds == null || enemyIds.Count == 0)
            return new();

        return new List<CombatantId>(enemyIds);
    }
}
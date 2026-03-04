using System.Collections.Generic;
using UnityEngine;

public class RandomEnemyTM : TargetMode
{
    public override List<CombatantId> GetTargetIds()
    {
        var enemyIds = EnemySystem.Instance != null ? EnemySystem.Instance.EnemyIds : null;
        if (enemyIds == null || enemyIds.Count == 0) return new();

        var id = enemyIds[Random.Range(0, enemyIds.Count)];
        return new() { id };
    }
}
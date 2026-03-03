using System.Collections.Generic;
using UnityEngine;

public class RandomEnemyTM : TargetMode
{
    public override List<CombatantId> GetTargetIds()
    {
        var enemies = EnemySystem.Instance.Enemies;
        if (enemies == null || enemies.Count == 0) return new();

        var enemy = enemies[Random.Range(0, enemies.Count)];
        return enemy ? new() { enemy.Id } : new();
    }
}
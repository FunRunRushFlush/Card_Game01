using System.Collections.Generic;

public class AllEnemiesTM : TargetMode
{
    public override List<CombatantId> GetTargetIds()
    {
        var enemies = EnemySystem.Instance.Enemies;
        var ids = new List<CombatantId>(enemies.Count);
        foreach (var e in enemies)
            if (e) ids.Add(e.Id);
        return ids;
    }
}
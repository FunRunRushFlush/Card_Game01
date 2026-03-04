using UnityEngine;

[System.Serializable]
public class EnemyCountAtLeastCondition : CardCondition
{
    [SerializeField] private int minEnemies = 1;

    public override bool IsMet(in CardPlayabilityContext context)
    {
        if (EnemySystem.Instance == null) return false;
        var ids = EnemySystem.Instance.EnemyIds;
        return ids != null && ids.Count >= minEnemies;
    }

    public override CardPlayFailReason GetFailReason(in CardPlayabilityContext context)
        => new(CardPlayFailCode.EnemyCountTooLow, $"Need at least {minEnemies} enemies");
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyMoves/MultiStrike")]
public class EnemyMultiStrikeMoveSO : EnemyMoveSO
{
    [Min(2)][SerializeField] private int hitCount = 2;

    public override IntentData GetIntent(EnemyView enemy)
    {
        int perHit = Mathf.Max(0, enemy.MultiAttackValue);
        int total = perHit * hitCount;

        return new IntentData
        {
            Icon = IntentIcon,
            ShowValue = true,
            Value = total,
            ValueText = $"{hitCount}×{perHit}"
        };
    }

    public override List<GameAction> BuildActions(EnemyView enemy)
    {
        int perHit = Mathf.Max(0, enemy.MultiAttackValue);

        var actions = new List<GameAction>(hitCount);
        for (int i = 0; i < hitCount; i++)
            actions.Add(new AttackHeroGA(enemy, perHit));

        return actions;
    }
}
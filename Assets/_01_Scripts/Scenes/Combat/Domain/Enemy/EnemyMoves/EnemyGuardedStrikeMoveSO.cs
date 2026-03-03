using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyMoves/GuardedStrike")]
public class EnemyGuardedStrikeMoveSO : EnemyMoveSO
{
    [SerializeField] private bool useEnemyBlockValue = true;
    [Min(0)][SerializeField] private int blockOverride = 4;

    public override IntentData GetIntent(EnemyView enemy)
        => IntentData.IconWithValue(IntentIcon, enemy.AttackValue);

    public override List<GameAction> BuildActions(EnemyView enemy)
    {
        int block = useEnemyBlockValue ? enemy.BlockValue : blockOverride;

        return new List<GameAction>
        {
            new AttackHeroGA(enemy),
            new AddBlockGA(block, new List<CombatantView> { enemy }, enemy)
        };
    }
}
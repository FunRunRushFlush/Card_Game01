using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyMoves/BlockSelf")]
public class EnemyBlockSelfMoveSO : EnemyMoveSO
{
    public override IntentData GetIntent(EnemyView enemy)
        => IntentData.IconWithValue(IntentIcon, enemy.BlockValue);

    public override List<GameAction> BuildActions(EnemyView enemy)
    {
        return new List<GameAction>
        {
            new AddBlockGA(enemy.BlockValue, new List<CombatantView> { enemy }, enemy)
        };
    }
}
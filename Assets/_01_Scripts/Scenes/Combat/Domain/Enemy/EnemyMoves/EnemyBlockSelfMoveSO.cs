using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyMoves/BlockSelf")]
public class EnemyBlockSelfMoveSO : EnemyMoveSO
{
    public override IntentData GetIntent(IEnemyActor enemy)
        => IntentData.IconWithValue(IntentIcon, enemy.BlockValue);

    public override List<GameAction> BuildActions(IEnemyActor enemy)
    {
        return new List<GameAction>
        {
            new AddBlockGA(enemy.BlockValue, new List<CombatantId> { enemy.Id }, enemy.Id)
        };
    }
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyMoves/AttackAndBurn")]
public class EnemyAttackAndBurnMoveSO : EnemyMoveSO
{
    public override IntentData GetIntent(IEnemyActor enemy)
        => IntentData.IconWithValue(IntentIcon, enemy.AttackValue);

    public override List<GameAction> BuildActions(IEnemyActor enemy)
    {
        return new List<GameAction>
        {
            new AttackHeroGA(enemy.Id),
            new AddStatusEffectGA(StatusEffectType.BURN, enemy.BurnValue, new List<CombatantId> { CombatantIds.Hero })
        };
    }
}
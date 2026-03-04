using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyMoves/AttackHero")]
public class EnemyAttackMoveSO : EnemyMoveSO
{
    public override IntentData GetIntent(IEnemyActor enemy)
        => IntentData.IconWithValue(IntentIcon, enemy.AttackValue);

    public override List<GameAction> BuildActions(IEnemyActor enemy)
        => new() { new AttackHeroGA(enemy.Id) };
}
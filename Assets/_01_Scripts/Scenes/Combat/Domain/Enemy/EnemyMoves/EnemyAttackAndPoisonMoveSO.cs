using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyMoves/AttackAndPoison")]
public class EnemyAttackAndPoisonMoveSO : EnemyMoveSO
{
    public override IntentData GetIntent(EnemyView enemy)
        => IntentData.IconWithValue(IntentIcon, enemy.AttackValue);

    public override List<GameAction> BuildActions(EnemyView enemy)
    {
        var actions = new List<GameAction>();

        actions.Add(new AttackHeroGA(enemy.Id));

        actions.Add(new AddStatusEffectGA(
            StatusEffectType.POISON,
            enemy.PoisonValue,
            new List<CombatantId> { HeroSystem.Instance.HeroView.Id }
        ));

        return actions;
    }
}

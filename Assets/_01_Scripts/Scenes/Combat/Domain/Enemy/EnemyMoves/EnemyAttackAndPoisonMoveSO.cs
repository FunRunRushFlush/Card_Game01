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


        actions.Add(new AttackHeroGA(enemy));

        actions.Add(new AddStatusEffectGA(
            StatusEffectType.POISON,
            enemy.PoisonValue,
            new List<CombatantView> { HeroSystem.Instance.HeroView }
        ));

        return actions;
    }
}

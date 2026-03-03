using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyMoves/AttackAndBurn")]
public class EnemyAttackAndBurnMoveSO : EnemyMoveSO
{
    public override IntentData GetIntent(EnemyView enemy)
        => IntentData.IconWithValue(IntentIcon, enemy.AttackValue);

    public override List<GameAction> BuildActions(EnemyView enemy)
    {
        var actions = new List<GameAction>();

        //Attack
        actions.Add(new AttackHeroGA(enemy));

        //Burn
        actions.Add(new AddStatusEffectGA(
            StatusEffectType.BURN,
            enemy.BurnValue,
            new List<CombatantView> { HeroSystem.Instance.HeroView }
        ));

        return actions;
    }
}

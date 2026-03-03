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

        actions.Add(new AttackHeroGA(enemy.Id)); // AttackHeroGA siehe unten

        actions.Add(new AddStatusEffectGA(
            StatusEffectType.BURN,
            enemy.BurnValue,
            new List<CombatantId> { HeroSystem.Instance.HeroView.Id }
        ));

        return actions;
    }
}

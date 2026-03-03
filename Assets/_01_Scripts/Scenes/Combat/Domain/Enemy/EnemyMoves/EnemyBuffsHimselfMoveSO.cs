using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Combat/EnemyMoves/SelfBuff")]
public class EnemyBuffsHimselfMoveSO : EnemyMoveSO
{
    [SerializeField] private StatusEffectType BuffType = StatusEffectType.STRENGTH;

    public override IntentData GetIntent(EnemyView enemy)
        => IntentData.IconWithValue(IntentIcon, enemy.StrengthValue);

    public override List<GameAction> BuildActions(EnemyView enemy)
    {
        var actions = new List<GameAction>();

        actions.Add(new AddStatusEffectGA(
      BuffType,
      enemy.StrengthValue,
      new List<CombatantView> {  enemy  }
         ));


        return actions;
    }
}
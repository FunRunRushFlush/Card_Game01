using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyMoves/SelfBuff")]
public class EnemyBuffsHimselfMoveSO : EnemyMoveSO
{
    [SerializeField] private StatusEffectType BuffType = StatusEffectType.STRENGTH;

    public override IntentData GetIntent(IEnemyActor enemy)
        => IntentData.IconWithValue(IntentIcon, GetBuffValue(enemy));

    public override List<GameAction> BuildActions(IEnemyActor enemy)
    {
        return new List<GameAction>
        {
            new AddStatusEffectGA(
                BuffType,
                GetBuffValue(enemy),
                new List<CombatantId> { enemy.Id }
            )
        };
    }

    private int GetBuffValue(IEnemyActor enemy)
    {
        return BuffType switch
        {
            StatusEffectType.STRENGTH => enemy.StrengthValue,
            StatusEffectType.WEAKNESS => enemy.WeaknessValue,
            _ => enemy.StrengthValue
        };
    }
}
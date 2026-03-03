using System;

public class OnEnemyAttackCondition : PerkCondition
{
    public override bool SubConditionIsMet(GameAction gameAction)
    {
        // if attacker is abaove x health, ...
        return true;
    }

    public override IDisposable SubscribeCondition(Action<GameAction> reaction)
    {
        return ActionSystem.SubscribeReaction<AttackHeroGA>(reaction, reactionTiming);
    }
}

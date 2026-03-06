using Game.Logging;
using System;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    private IDisposable enemyTurnPreSub;
    private IDisposable enemyTurnPostSub;

    void OnEnable()
    {
        enemyTurnPreSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        enemyTurnPostSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        enemyTurnPreSub?.Dispose();
        enemyTurnPreSub = null;

        enemyTurnPostSub?.Dispose();
        enemyTurnPostSub = null;
    }

    public void Setup(CombatantId id, HeroData heroData)
    {
        CombatDomainEventBus.Publish(new HeroSpawnedEvent(id, heroData));
    }

    private void EnemyTurnPreReaction(EnemyTurnGA _)
    {
        ActionSystem.Instance.AddReaction(new DiscardAllCardsGA());
    }

    private void EnemyTurnPostReaction(EnemyTurnGA _)
    {
        var heroContext = CombatContextService.Instance.Hero;
        if (heroContext == null || heroContext.DrawPerTurn < 1)
        {
            Log.Error(LogArea.Combat, () => "CombatContext.Instance.Hero.DrawPerTurn missing or invalid.", this);
            return;
        }

        ActionSystem.Instance.AddReaction(new DrawCardsGA(heroContext.DrawPerTurn));
    }
}
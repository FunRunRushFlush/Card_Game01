using Game.Logging;
using Game.Scenes.Core;
using System;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    [field: SerializeField] public HeroView HeroView { get; private set; }

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

    public void Setup(HeroData heroData)
    {
        HeroView.Setup(heroData);
    }

    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        ActionSystem.Instance.AddReaction(new DiscardAllCardsGA());
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        var heroContext = CombatContextSystem.Instance.Hero;
        if (heroContext == null || heroContext.DrawPerTurn < 1)
        {
            Log.Error(LogArea.Combat, () => "CombatContext.Instance.Hero.DrawPerTurn missing or invalid.", this);
            return;
        }

        ActionSystem.Instance.AddReaction(new DrawCardsGA(heroContext.DrawPerTurn));
    }
}
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

    //private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    //{
    //    int burnStacks = HeroView.GetStatusEffectStacks(StatusEffectType.BURN);
    //    if (burnStacks > 0)
    //    {
    //        ApplyBurnGA applyBurnGA = new(burnStacks, HeroView);
    //        ActionSystem.Instance.AddReaction(applyBurnGA);
    //    }

    //    int poisonStacks = HeroView.GetStatusEffectStacks(StatusEffectType.POISON);
    //    if (poisonStacks > 0)
    //        ActionSystem.Instance.AddReaction(new ApplyPoisonGA(poisonStacks, HeroView));

    //    DiscardAllCardsGA discardAllCardsGA = new();
    //    ActionSystem.Instance.AddReaction(discardAllCardsGA);
    //}
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        ActionSystem.Instance.AddReaction(new DiscardAllCardsGA());
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        var herodata = CoreManager.Instance.Session.Hero.Data;
        
        if (herodata == null || herodata.DrawPerTurn < 1)
        {
            Debug.LogError("[HeroSystem] CoreManager.Instance.Session.Hero.Data.DrawPerTurn missing.");
            return;
        }
        ActionSystem.Instance.AddReaction(new DrawCardsGA(herodata.DrawPerTurn));
    }

}
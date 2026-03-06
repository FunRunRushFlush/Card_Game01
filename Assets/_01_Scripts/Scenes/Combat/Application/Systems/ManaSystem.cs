using Game.Scenes.Core;
using System;
using System.Collections;

public class ManaSystem : Singleton<ManaSystem>
{
    private int maxMana;
    private int currentMana;

    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;

    private IDisposable enemyTurnPostSub;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendManaGA>(SpendManaPerformer);
        ActionSystem.AttachPerformer<RefillManaGA>(RefillManaPerformer);

        enemyTurnPostSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);

        CombatContextHelpers.SubscribeInitialized(OnCombatInitialized);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendManaGA>();
        ActionSystem.DetachPerformer<RefillManaGA>();

        enemyTurnPostSub?.Dispose();
        enemyTurnPostSub = null;

        CombatContextHelpers.UnsubscribeInitialized(OnCombatInitialized);
    }

    private void OnCombatInitialized()
    {
        var hero = CombatContextService.Instance.Hero;
        maxMana = hero.MaxMana;
        currentMana = maxMana;

        PublishManaChanged();
    }

    public bool HasEnoughMana(int mana)
    {
        return currentMana >= mana;
    }

    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    {
        currentMana -= spendManaGA.Amount;

        if (currentMana < 0)
            currentMana = 0;

        PublishManaChanged();
        yield return null;
    }

    private IEnumerator RefillManaPerformer(RefillManaGA ga)
    {
        maxMana = CombatContextService.Instance.Hero.MaxMana;
        currentMana = maxMana;

        PublishManaChanged();
        yield return null;
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        RefillManaGA refillManaGA = new();
        ActionSystem.Instance.AddReaction(refillManaGA);
    }

    private void PublishManaChanged()
    {
        CombatDomainEventBus.Publish(
            new ManaChangedEvent(currentMana, maxMana)
        );
    }
}
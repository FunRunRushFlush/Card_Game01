using Game.Scenes.Core;
using System;
using System.Collections;
using UnityEngine;

public class ManaSystem : Singleton<ManaSystem>
{
    [SerializeField] private ManaUI manaUI;
    private int maxMana;
    private int currentMana;


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
        manaUI.UpdateManaText(currentMana);
    }

    public bool HasEnoughMana(int mana)
    {
        return currentMana >= mana;
    }
    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    {
        currentMana -= spendManaGA.Amount;
        manaUI.UpdateManaText(currentMana);
        yield return null;
    }

    private IEnumerator RefillManaPerformer(RefillManaGA ga)
    {
        maxMana = CombatContextService.Instance.Hero.MaxMana;
        currentMana = maxMana;
        manaUI.UpdateManaText(currentMana);
        yield return null;
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        RefillManaGA refillManaGA = new();
        ActionSystem.Instance.AddReaction(refillManaGA);
    }
}
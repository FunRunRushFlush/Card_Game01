using System;
using UnityEngine;

public class StatusTickSystem : MonoBehaviour
{
    private IDisposable enemyTurnPreSub;

    private void OnEnable()
    {
        enemyTurnPreSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnPre, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        enemyTurnPreSub?.Dispose();
        enemyTurnPreSub = null;
    }

    private void OnEnemyTurnPre(EnemyTurnGA _)
    {
        // Hero
        TickCombatant(HeroSystem.Instance?.HeroView);

        // Enemies (Snapshot, falls Liste sich während Reactions ändert)
        var enemies = EnemySystem.Instance?.Enemies;
        if (enemies == null) return;

        for (int i = 0; i < enemies.Count; i++)
            TickCombatant(enemies[i]);
    }

    private void TickCombatant(CombatantView target)
    {
        if (!target || target.CurrentHealth <= 0)
            return;

        int burn = target.GetStatusEffectStacks(StatusEffectType.BURN);
        if (burn > 0)
            ActionSystem.Instance.AddReaction(new ApplyBurnGA(burn, target));

        int poison = target.GetStatusEffectStacks(StatusEffectType.POISON);
        if (poison > 0)
            ActionSystem.Instance.AddReaction(new ApplyPoisonGA(poison, target));
    }
}
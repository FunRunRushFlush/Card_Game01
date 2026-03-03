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
        var hero = HeroSystem.Instance != null ? HeroSystem.Instance.HeroView : null;
        if (hero != null)
            TickCombatant(hero.Id);

        // Enemies (Snapshot, falls Liste sich während Reactions ändert)
        var enemies = EnemySystem.Instance?.Enemies;
        if (enemies == null) return;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (!enemies[i]) continue;
            TickCombatant(enemies[i].Id);
        }
    }

    private void TickCombatant(CombatantId targetId)
    {
        if (!CombatContextService.Instance.State.TryGet(targetId, out var st) || st.Health <= 0)
            return;

        int burn = st.GetStatus(StatusEffectType.BURN);
        if (burn > 0)
            ActionSystem.Instance.AddReaction(new ApplyBurnGA(burn, targetId));

        int poison = st.GetStatus(StatusEffectType.POISON);
        if (poison > 0)
            ActionSystem.Instance.AddReaction(new ApplyPoisonGA(poison, targetId));
    }
}
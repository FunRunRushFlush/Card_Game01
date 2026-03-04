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
        // Hero (no view access)
        TickCombatant(CombatantIds.Hero);

        // Enemies (no view access)
        var enemySystem = EnemySystem.Instance;
        if (enemySystem == null) return;

        foreach (var enemyId in enemySystem.EnemyIds)
            TickCombatant(enemyId);
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
using System;
using System.Collections;
using UnityEngine;

public class BlockSystem : Singleton<BlockSystem>
{
    private IDisposable enemyTurnPreSub;
    private IDisposable enemyTurnPostSub;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddBlockGA>(AddBlockPerformer);

        enemyTurnPreSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnPre, ReactionTiming.PRE);
        enemyTurnPostSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnPost, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddBlockGA>();

        enemyTurnPreSub?.Dispose();
        enemyTurnPreSub = null;

        enemyTurnPostSub?.Dispose();
        enemyTurnPostSub = null;
    }

    private IEnumerator AddBlockPerformer(AddBlockGA ga)
    {
        foreach (var targetId in ga.Targets)
        {
            if (!CombatContextService.Instance.State.TryGet(targetId, out var targetState))
                continue;

            targetState.AddBlock(ga.Amount);

            CombatDomainEventBus.Publish(new CombatantStateChangedEvent(targetId));
            yield return null;
        }
    }

    private void OnEnemyTurnPre(EnemyTurnGA _)
    {
        var enemySystem = EnemySystem.Instance;
        if (enemySystem == null) return;

        foreach (var enemyId in enemySystem.EnemyIds)
        {
            if (!CombatContextService.Instance.State.TryGet(enemyId, out var st))
                continue;

            st.ClearBlock();
            CombatDomainEventBus.Publish(new CombatantStateChangedEvent(enemyId));
        }
    }

    private void OnEnemyTurnPost(EnemyTurnGA _)
    {
        var heroId = CombatantIds.Hero;

        if (!CombatContextService.Instance.State.TryGet(heroId, out var st))
            return;

        st.ClearBlock();
        CombatDomainEventBus.Publish(new CombatantStateChangedEvent(heroId));
    }
}
using System;
using System.Collections;
using UnityEngine;

public class BlockSystem : Singleton<BlockSystem>
{
    [SerializeField] private CombatantViewRegistry viewRegistry;


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

            if (viewRegistry && viewRegistry.TryGet(targetId, out var view) && view)
                view.Render(targetState);

            yield return null;
        }
    }

    private void OnEnemyTurnPre(EnemyTurnGA _)
    {
        var enemySystem = EnemySystem.Instance;
        if (enemySystem == null) return;

        foreach (var enemyView in enemySystem.Enemies)
        {
            if (!enemyView) continue;
            var id = enemyView.Id;

            if (CombatContextService.Instance.State.TryGet(id, out var st))
            {
                st.ClearBlock();
                if (viewRegistry && viewRegistry.TryGet(id, out var view) && view)
                    view.Render(st);
            }
        }
    }

    private void OnEnemyTurnPost(EnemyTurnGA _)
    {
        var hero = HeroSystem.Instance?.HeroView;
        if (!hero) return;

        var id = hero.Id;
        if (CombatContextService.Instance.State.TryGet(id, out var st))
        {
            st.ClearBlock();
            if (viewRegistry && viewRegistry.TryGet(id, out var view) && view)
                view.Render(st);
        }
    }
}
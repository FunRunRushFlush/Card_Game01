using DG.Tweening;
using Game.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyBoardView enemyBoardView;
    [SerializeField] private CombatantViewRegistry viewRegistry;

    public List<EnemyView> Enemies => enemyBoardView.EnemyViews;

    public event Action AllEnemiesDefeated;

    private IDisposable enemyTurnPostSub;

    // Logic-only runtime models (no views inside)
    private readonly Dictionary<CombatantId, EnemyRuntime> runtimeById = new();

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);

        // After EnemyTurn: advance AI and choose next intent (logic), then render in views (presentation)
        enemyTurnPostSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(_ =>
        {
            foreach (var runtime in runtimeById.Values)
            {
                runtime.AIState?.AdvanceTurn();
                runtime.ChooseNextIntent();
            }

            RenderAllIntents();
        }, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();

        enemyTurnPostSub?.Dispose();
        enemyTurnPostSub = null;
    }

    public void Setup(List<EnemyData> enemyDatas)
    {
        if (enemyBoardView == null)
        {
            Log.Error(LogArea.Combat, () => "[EnemySystem] EnemyBoardView reference is missing.", this);
            return;
        }

        runtimeById.Clear();

        // Spawn views + create runtime logic (deterministic ids: 1..n)
        for (int i = 0; i < enemyDatas.Count; i++)
        {
            var data = enemyDatas[i];
            if (data == null) continue;

            var id = CombatantIds.Enemy(i);

            // Create runtime (logic)
            int seed = id.Value; // if CombatantId has different API, replace accordingly
            var runtime = new EnemyRuntime(id, data, seed);
            runtimeById[id] = runtime;

            // Spawn view (presentation)
            enemyBoardView.AddEnemy(data, id);
        }

        // Choose + render initial intents
        foreach (var runtime in runtimeById.Values)
            runtime.ChooseNextIntent();

        RenderAllIntents();
    }

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA action)
    {
        if (AreAllEnemiesDefeated())
            yield break;

        // Keep the execution order stable using the current view order (1..n).
        var enemiesSnapshot = new List<EnemyView>(enemyBoardView.EnemyViews);

        foreach (var enemyView in enemiesSnapshot)
        {
            if (!enemyView) continue;

            if (!runtimeById.TryGetValue(enemyView.Id, out var runtime) || runtime == null)
                continue;

            var actions = runtime.BuildActionsFromCurrentIntent();
            if (actions == null || actions.Count == 0)
                continue;

            foreach (var ga in actions)
                ActionSystem.Instance.AddReaction(ga);
        }

        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA ga)
    {
        // Find attacker view for animation
        if (viewRegistry == null || !viewRegistry.TryGet(ga.AttackerId, out var attackerView) || !attackerView)
            yield break;

        // Optional: animate attacker lunge (presentation only)
        var tween = attackerView.transform.DOMoveX(attackerView.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();

        if (!attackerView) yield break;

        attackerView.transform.DOMoveX(attackerView.transform.position.x + 1f, 0.25f);

        // Hero id should not come from HeroView anymore
        var heroId = CombatantIds.Hero;

        // Damage value comes from runtime, not from view
        int damage = ga.DamageOverride ?? GetAttackDamageFromRuntime(ga.AttackerId);
        if (damage <= 0)
            yield break;

        ActionSystem.Instance.AddReaction(
            new DealDamageGA(damage, new List<CombatantId> { heroId }, ga.CasterId)
        );
    }

    private int GetAttackDamageFromRuntime(CombatantId attackerId)
    {
        if (runtimeById.TryGetValue(attackerId, out var runtime) && runtime != null)
            return runtime.AttackValue;

        return 0;
    }

    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        if (killEnemyGA == null)
            throw new ArgumentNullException(nameof(killEnemyGA));

        // Remove runtime first
        runtimeById.Remove(killEnemyGA.EnemyId);

        // Find the EnemyView by id (presentation)
        EnemyView enemyView = null;
        foreach (var e in enemyBoardView.EnemyViews)
        {
            if (!e) continue;
            if (e.Id.Value == killEnemyGA.EnemyId.Value) { enemyView = e; break; }
        }

        if (enemyView == null)
            yield break;

        yield return enemyBoardView.RemoveEnemy(enemyView);

        if (AreAllEnemiesDefeated())
            AllEnemiesDefeated?.Invoke();
    }

    private void RenderAllIntents()
    {
        if (enemyBoardView == null || enemyBoardView.EnemyViews == null)
            return;

        foreach (var view in enemyBoardView.EnemyViews)
        {
            if (!view) continue;

            if (runtimeById.TryGetValue(view.Id, out var runtime) && runtime != null)
                view.RenderIntent(runtime.CurrentIntent);
            else
                view.RenderIntent(default);
        }
    }

    public bool AreAllEnemiesDefeated()
    {
        return enemyBoardView == null
               || enemyBoardView.EnemyViews == null
               || enemyBoardView.EnemyViews.Count == 0;
    }
}
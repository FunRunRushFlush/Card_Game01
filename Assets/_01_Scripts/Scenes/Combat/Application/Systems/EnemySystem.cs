using Game.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    public IReadOnlyList<CombatantId> EnemyIds => enemyIds;

    public event Action AllEnemiesDefeated;

    private IDisposable enemyTurnPostSub;

    private readonly List<CombatantId> enemyIds = new();
    private readonly Dictionary<CombatantId, EnemyRuntime> runtimeById = new();

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);

        enemyTurnPostSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(_ =>
        {
            foreach (var runtime in runtimeById.Values)
            {
                runtime.AIState?.AdvanceTurn();
                runtime.ChooseNextIntent();
                CombatEventBus.Publish(new EnemyIntentChangedEvent(runtime.Id, runtime.CurrentIntent));
            }
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
        runtimeById.Clear();
        enemyIds.Clear();

        for (int i = 0; i < enemyDatas.Count; i++)
        {
            var data = enemyDatas[i];
            if (data == null) continue;

            var id = CombatantIds.Enemy(i);
            enemyIds.Add(id);

            int seed = id.Value;
            var runtime = new EnemyRuntime(id, data, seed);
            runtimeById[id] = runtime;

            // Presentation: spawn request
            CombatEventBus.Publish(new EnemySpawnRequestedEvent(id, data));
        }

        // initial intents
        foreach (var runtime in runtimeById.Values)
        {
            runtime.ChooseNextIntent();
            CombatEventBus.Publish(new EnemyIntentChangedEvent(runtime.Id, runtime.CurrentIntent));
        }
    }

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA action)
    {
        if (runtimeById.Count == 0)
            yield break;

        // iterate deterministic by EnemyIds
        foreach (var id in enemyIds)
        {
            if (!runtimeById.TryGetValue(id, out var runtime) || runtime == null)
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
        var token = PresentationGate.NewToken();
        CombatEventBus.Publish(new AttackLungeRequestedEvent(ga.AttackerId, token));

        // Wait until presentation signals completion (animation length decides)
        yield return PresentationGate.Wait(token);

        // Now apply damage
        var heroId = CombatantIds.Hero;
        int damage = ga.DamageOverride ?? GetAttackDamageFromRuntime(ga.AttackerId);
        if (damage > 0)
        {

            ActionSystem.Instance.AddReaction(new DealDamageGA(damage, new List<CombatantId> { heroId }, ga.CasterId));
        }
    }
    private int GetAttackDamageFromRuntime(CombatantId attackerId)
    {
        if (runtimeById.TryGetValue(attackerId, out var runtime) && runtime != null)
            return runtime.AttackValue;

        return 0;
    }

    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        runtimeById.Remove(killEnemyGA.EnemyId);
        enemyIds.RemoveAll(x => x.Value == killEnemyGA.EnemyId.Value);

        CombatEventBus.Publish(new EnemyDiedEvent(killEnemyGA.EnemyId));

        if (runtimeById.Count == 0)
            AllEnemiesDefeated?.Invoke();

        yield return null;
    }

    public bool AreAllEnemiesDefeated()
    => enemyIds == null || enemyIds.Count == 0;
    public bool TryGetRuntime(CombatantId id, out EnemyRuntime runtime)
        => runtimeById.TryGetValue(id, out runtime);
}
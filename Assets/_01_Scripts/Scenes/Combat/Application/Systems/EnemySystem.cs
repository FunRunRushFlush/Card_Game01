using DG.Tweening;
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

    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);

        // TODO: ?
        // Nach dem EnemyTurn: nächsten Intent wählen
        enemyTurnPostSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(_ =>
        {
            foreach (var enemy in enemyBoardView.EnemyViews)
            {
                if (!enemy) continue;
                enemy.AIState?.AdvanceTurn();
                enemy.ChooseNextIntent();
            }
        }, ReactionTiming.POST);
    }


    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();

        enemyTurnPostSub?.Dispose();
        enemyTurnPostSub = null;
    }
    public void Setup(List<EnemyData> enemyDatas)
    {
        // Clear current board
        foreach (EnemyData data in enemyDatas)
        {
            // Deterministic ids: 1..n
            var id = new CombatantId(enemyBoardView.EnemyViews.Count + 1);
            enemyBoardView.AddEnemy(data, id);
        }

        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            if (!enemy) continue;
            enemy.ChooseNextIntent();
        }
    }

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA action)
    {
        if (AreAllEnemiesDefeated())
            yield break;

        var enemiesSnapshot = new List<EnemyView>(enemyBoardView.EnemyViews);

        foreach (var enemy in enemiesSnapshot)
        {
            if (!enemy) continue;

            var actions = enemy.BuildActionsFromCurrentIntent();
            foreach (var ga in actions)
            {
                ActionSystem.Instance.AddReaction(ga);
            }
        }

        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA ga)
    {
        if (viewRegistry == null || !viewRegistry.TryGet(ga.AttackerId, out var attackerView) || !attackerView)
            yield break;

        // anim
        var tween = attackerView.transform.DOMoveX(attackerView.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();

        if (!attackerView) yield break;

        attackerView.transform.DOMoveX(attackerView.transform.position.x + 1f, 0.25f);

        var heroId = HeroSystem.Instance.HeroView.Id;

        // damage value (solange AttackValue noch im View steckt)
        int damage = ga.DamageOverride ?? ((EnemyView)attackerView).AttackValue;

        ActionSystem.Instance.AddReaction(
            new DealDamageGA(damage, new List<CombatantId> { heroId }, ga.CasterId)
        );
    }


    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        if (killEnemyGA == null)
            throw new ArgumentNullException(nameof(killEnemyGA));

        // Find the EnemyView by id
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
        {
            AllEnemiesDefeated?.Invoke();
        }
    }


    public bool AreAllEnemiesDefeated()
    {

        return enemyBoardView == null
               || enemyBoardView.EnemyViews == null
               || enemyBoardView.EnemyViews.Count == 0;
    }
}
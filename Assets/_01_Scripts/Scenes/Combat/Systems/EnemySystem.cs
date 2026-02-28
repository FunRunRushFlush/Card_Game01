using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyBoardView enemyBoardView;
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
        foreach (EnemyData data in enemyDatas)
        {
            enemyBoardView.AddEnemy(data);
        }


        // TODO:
        // Optional: falls AddEnemy/Setup nicht schon ChooseNextIntent macht,
        // hier nochmal sicherstellen, dass jeder Enemy einen Intent hat.
        foreach (var enemy in enemyBoardView.EnemyViews)
        { 
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

            int burnStacks = enemy.GetStatusEffectStacks(StatusEffectType.BURN);
            if (burnStacks > 0)
                ActionSystem.Instance.AddReaction(new ApplyBurnGA(burnStacks, enemy));

            int poisonStacks = enemy.GetStatusEffectStacks(StatusEffectType.POISON);
            if (poisonStacks > 0)
                ActionSystem.Instance.AddReaction(new ApplyPoisonGA(poisonStacks, enemy));

            // HINWEIS:
            // Aktionen trotzdem enqueue-n, aber Performer müssen robust sein!!! (siehe AttackHeroPerformer)
            var actions = enemy.BuildActionsFromCurrentIntent();
            foreach (var ga in actions)
            {
                ActionSystem.Instance.AddReaction(ga);
            }
        }

        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;

        if (!attacker)
        {
            yield break;
        }

        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        
        if(!attacker)
        {
            yield break;
        }

        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);
        if (!HeroSystem.Instance || !HeroSystem.Instance.HeroView)
            yield break;

        int damage = attackHeroGA.DamageOverride ?? attacker.AttackValue;
        DealDamageGA dealDamageGA = new(damage, new() { HeroSystem.Instance.HeroView }, attackHeroGA.Caster);
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }


    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        if (killEnemyGA == null || killEnemyGA.EnemyView == null)
        {
            throw new ArgumentNullException($"{nameof(KillEnemyPerformer)}");
        }

        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);


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
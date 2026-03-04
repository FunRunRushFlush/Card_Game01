using DG.Tweening;
using Game.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class CombatPresentationController : Singleton<CombatPresentationController>
{
    [Header("Views")]
    [SerializeField] private EnemyBoardView enemyBoardView;
    [SerializeField] private CombatantViewRegistry viewRegistry;
    [SerializeField] private HeroView heroView;

    [Header("FX")]
    [SerializeField] private GameObject damageVFX;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;

    public HeroView HeroView => heroView;

    public IReadOnlyList<EnemyView> EnemyViews
        => enemyBoardView != null ? enemyBoardView.EnemyViews : null;

    public bool ContainsEnemyView(EnemyView view)
    {
        var list = EnemyViews;
        return list != null && view != null && list.Contains(view);
    }

    public bool TryGetEnemyView(CombatantId id, out EnemyView view)
    {
        view = null;
        var list = EnemyViews;
        if (list == null) return false;

        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            if (!e) continue;
            if (e.Id.Value == id.Value)
            {
                view = e;
                return true;
            }
        }

        return false;
    }

    private void OnEnable()
    {
        CombatEventBus.OnEvent += HandleEvent;
    }

    private void OnDisable()
    {
        CombatEventBus.OnEvent -= HandleEvent;
    }

    private void HandleEvent(ICombatEvent e)
    {
        switch (e)
        {
            case HeroSpawnRequestedEvent hero:
                SpawnHero(hero.HeroId, hero.Data);
                break;

            case EnemySpawnRequestedEvent spawn:
                SpawnEnemy(spawn.Data, spawn.EnemyId);
                break;

            case EnemyIntentChangedEvent intent:
                RenderEnemyIntent(intent.EnemyId, intent.Intent);
                break;

            case EnemyDiedEvent died:
                StartCoroutine(RemoveEnemy(died.EnemyId));
                break;

            case AttackLungeRequestedEvent lunge:
                StartCoroutine(PlayEnemyAttackLunge(lunge.AttackerId, lunge.Token));
                break;

            case DamageAppliedEvent dmg:
                PlayDamageFeedback(dmg.TargetId);
                break;

            case CombatantStateChangedEvent changed:
                RenderCombatant(changed.Id);
                break;

            case CardPlayPresentationRequestedEvent cardPlay:
                // Use sequence-based playback (targetId currently not part of the event)
                StartCoroutine(PlayCardSequence(cardPlay.CardView, cardPlay.DiscardPosition, targetId: null, cardPlay.Token));
                break;
        }
    }

    // -----------------------
    // Spawn / Render helpers
    // -----------------------

    private void SpawnHero(CombatantId id, HeroData data)
    {
        if (!heroView)
        {
            Log.Error(LogArea.Combat, () => "[CombatPresentationController] HeroView missing.", this);
            return;
        }

        heroView.AssignId(id);
        viewRegistry?.Register(heroView);
        heroView.Setup(data);

        RenderCombatant(id);
    }

    private void SpawnEnemy(EnemyData data, CombatantId id)
    {
        if (enemyBoardView == null)
        {
            Log.Error(LogArea.Combat, () => "[CombatPresentationController] EnemyBoardView is missing.", this);
            return;
        }

        enemyBoardView.AddEnemy(data, id);
        RenderCombatant(id);
    }

    private void RenderEnemyIntent(CombatantId enemyId, IntentData intent)
    {
        var list = EnemyViews;
        if (list == null) return;

        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            if (!e) continue;
            if (e.Id.Value != enemyId.Value) continue;

            e.RenderIntent(intent);
            return;
        }
    }

    private void RenderCombatant(CombatantId id)
    {
        if (!viewRegistry) return;

        if (!CombatContextService.Instance.State.TryGet(id, out var st))
            return;

        if (viewRegistry.TryGet(id, out var view) && view)
            view.Render(st);
    }

    private void PlayDamageFeedback(CombatantId targetId)
    {
        if (!viewRegistry) return;

        if (!viewRegistry.TryGet(targetId, out var view) || !view)
            return;

        view.PlayHitFeedback();

        if (damageVFX)
            Instantiate(damageVFX, view.HitPointWorld, Quaternion.identity);
    }

    public IEnumerator RemoveEnemy(CombatantId id)
    {
        var list = EnemyViews;
        if (list == null)
            yield break;

        EnemyView enemyView = null;
        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            if (!e) continue;
            if (e.Id.Value == id.Value) { enemyView = e; break; }
        }

        if (enemyView == null)
            yield break;

        yield return enemyBoardView.RemoveEnemy(enemyView);
    }

    // -----------------------
    // Audio / Position helpers
    // -----------------------

    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;

        if (sfxSource != null) sfxSource.PlayOneShot(clip);
        else AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }

    public bool TryGetCombatantWorldPosition(CombatantId id, out Vector3 pos)
    {
        pos = default;

        if (viewRegistry != null && viewRegistry.TryGet(id, out var view) && view)
        {
            pos = view.transform.position;
            return true;
        }

        return false;
    }

    // -----------------------
    // Presentation coroutines
    // -----------------------

    public IEnumerator PlayEnemyAttackLunge(CombatantId attackerId, int token)
    {
        try
        {
            if (!viewRegistry || !viewRegistry.TryGet(attackerId, out var attackerView) || !attackerView)
                yield break;

            var startPos = attackerView.transform.position;

            attackerView.transform.DOKill();

            var lunge = attackerView.transform.DOMoveX(startPos.x - 1f, 0.15f);
            yield return lunge.WaitForCompletion();

            if (!attackerView)
                yield break;

            var back = attackerView.transform.DOMoveX(startPos.x, 0.25f);
            yield return back.WaitForCompletion();
        }
        finally
        {
            PresentationGate.Complete(token);
        }
    }

    /// <summary>
    /// Plays the card's animation sequence (if set). Falls back to default fly+shrink.
    /// Destroys the card view at the end.
    /// </summary>
    public IEnumerator PlayCardSequence(CardView cardView, Vector3 discardPos, CombatantId? targetId, int token)
    {
        try
        {
            if (!cardView)
                yield break;

            cardView.transform.DOKill();

            var seq = cardView.Card?.Data?.AnimationSequence;

            if (seq == null || seq.Steps == null || seq.Steps.Count == 0)
            {
                yield return PlayCardFallback(cardView, discardPos);
                yield break;
            }

            var ctx = new CardAnimContext(this, cardView, discardPos, targetId);

            for (int i = 0; i < seq.Steps.Count; i++)
            {
                if (!cardView) yield break;

                var step = seq.Steps[i];
                if (step == null) continue;

                yield return step.Play(ctx);
            }

            if (cardView)
                Destroy(cardView.gameObject);
        }
        finally
        {
            PresentationGate.Complete(token);
        }
    }

    private IEnumerator PlayCardFallback(CardView cardView, Vector3 discardPos)
    {
        if (!cardView) yield break;

        var pop = cardView.transform.DOScale(1.05f, 0.08f);
        yield return pop.WaitForCompletion();

        if (!cardView) yield break;

        var fly = cardView.transform.DOMove(discardPos, 0.18f);
        cardView.transform.DOScale(Vector3.zero, 0.18f);

        yield return fly.WaitForCompletion();

        if (cardView)
            Destroy(cardView.gameObject);
    }
}
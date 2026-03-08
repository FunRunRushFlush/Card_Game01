using DG.Tweening;
using System.Collections;
using UnityEngine;

public sealed class CombatPresentationEventRouter : MonoBehaviour
{
    [SerializeField] private CombatPresentationController presentationController;
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;
    [SerializeField] private ManaUI manaUI;
    [SerializeField] private PerksUI perksUI;
    [SerializeField] private ComboPointsUI comboPointsUI;

    private void OnEnable()
    {
        CombatDomainEventBus.OnEvent += HandleEvent;
    }

    private void OnDisable()
    {
        CombatDomainEventBus.OnEvent -= HandleEvent;
    }

    private void HandleEvent(ICombatDomainEvent e)
    {
        switch (e)
        {
            case HeroSpawnedEvent hero:
                presentationController.SpawnHeroView(hero.HeroId, hero.Data);
                break;

            case EnemySpawnedEvent enemy:
                presentationController.SpawnEnemyView(enemy.Data, enemy.EnemyId);
                break;

            case EnemyIntentChangedEvent intent:
                presentationController.RenderEnemyIntentView(intent.EnemyId, intent.Intent);
                break;

            case EnemyDiedEvent died:
                StartCoroutine(presentationController.RemoveEnemyView(died.EnemyId));
                break;

            case DamageAppliedEvent dmg:
                presentationController.PlayDamageFeedbackView(dmg.TargetId);
                break;

            case CombatantStateChangedEvent changed:
                presentationController.RenderCombatantView(changed.Id);
                break;

            case StatusTickVisualRequestedEvent statusTick:
                presentationController.PlayStatusTickVfx(statusTick.TargetId, statusTick.StatusType);
                break;

            case ManaChangedEvent manaChanged:
                if (manaUI != null)
                    manaUI.UpdateManaText(manaChanged.CurrentMana, manaChanged.MaxMana);
                break;

            case ComboPointsChangedEvent comboChanged:
                if (comboPointsUI != null)
                    comboPointsUI.Render(comboChanged.CurrentComboPoints, comboChanged.MaxComboPoints);
                break;

            case PerkAddedEvent perkAdded:
                if (perksUI != null)
                    perksUI.AddPerkUI(perkAdded.Perk);
                break;

            case PerkRemovedEvent perkRemoved:
                if (perksUI != null)
                    perksUI.RemovePerkUI(perkRemoved.Perk);
                break;

            case CardDrawnToHandEvent drawn:
                StartCoroutine(AddCardToHand(drawn));
                break;

            case CardDiscardedFromHandEvent discarded:
                StartCoroutine(DiscardCardFromHand(discarded));
                break;

            case AttackDeclaredEvent attack:
                StartCoroutine(PlayAttackSequence(attack));
                break;

            case CardPlayedEvent cardPlayed:
                StartCoroutine(PlayCardSequence(cardPlayed));
                break;
        }
    }

    private IEnumerator AddCardToHand(CardDrawnToHandEvent e)
    {
        if (CardViewCreator.Instance == null || handView == null || drawPilePoint == null)
            yield break;

        var cardView = CardViewCreator.Instance.CreateCardView(
            e.Card,
            drawPilePoint.position,
            drawPilePoint.rotation,
            handView.HandCardContrainer.transform
        );

        yield return handView.AddCard(cardView);
    }

    private IEnumerator DiscardCardFromHand(CardDiscardedFromHandEvent e)
    {
        if (handView == null || discardPilePoint == null)
            yield break;

        var cardView = handView.GetCardViewByCardRuntimeId(e.CardRuntimeId);

        if (cardView == null)
            yield return null;

        if (cardView == null)
            cardView = handView.GetCardViewByCardRuntimeId(e.CardRuntimeId);

        if (cardView == null)
            yield break;

        handView.RemoveCard(cardView.Card);

        cardView.transform.DOScale(Vector3.zero, 0.15f);
        var tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);

        yield return tween.WaitForCompletion();

        if (cardView != null)
            Destroy(cardView.gameObject);
    }

    private IEnumerator PlayAttackSequence(AttackDeclaredEvent e)
    {
        try
        {
            yield return presentationController.PlayEnemyAttackLunge(e.AttackerId);
        }
        finally
        {
            PresentationGate.Complete(e.SequenceId);
        }
    }

    private IEnumerator PlayCardSequence(CardPlayedEvent e)
    {
        try
        {
            var cardView = handView.GetCardViewByCardRuntimeId(e.CardRuntimeId);

            if (cardView == null)
                yield break;

            yield return presentationController.PlayCardSequence(
                cardView,
                discardPilePoint.position,
                e.ManualTargetId
            );
        }
        finally
        {
            PresentationGate.Complete(e.SequenceId);
        }
    }
}
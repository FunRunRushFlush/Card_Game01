using Game.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    private int MaxHandSizeSafty = 99;

    private readonly List<Card> hand = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> drawPile = new();
    private readonly List<Card> banishPile = new();

    public event Action<int> DrawPileCountChanged;
    public event Action<int> DiscardPileCountChanged;
    public event Action<int> HandCountChanged;
    public event Action<int> BanishPileCountChanged;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardCardsGA>(DiscardCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
    }

    private void UpdatePileCounts()
    {
        DrawPileCountChanged?.Invoke(drawPile.Count);
        DiscardPileCountChanged?.Invoke(discardPile.Count);
        HandCountChanged?.Invoke(hand.Count);
        BanishPileCountChanged?.Invoke(banishPile.Count);
    }

    public void Setup(List<CardData> startingDeck)
    {
        hand.Clear();
        discardPile.Clear();
        drawPile.Clear();
        banishPile.Clear();

        foreach (var cardData in startingDeck)
        {
            if (cardData == null)
                continue;

            Card card = new Card(cardData);
            drawPile.Add(card);
        }

        drawPile.Shuffle();
        UpdatePileCounts();
    }

    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        int actualDrawCount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notEnoughCards = drawCardsGA.Amount - actualDrawCount;

        for (int i = 0; i < actualDrawCount; i++)
            yield return DrawCard();

        if (notEnoughCards > 0)
        {
            RefillDeck();

            for (int i = 0; i < notEnoughCards; i++)
                yield return DrawCard();
        }
    }

    private IEnumerator DiscardCardsPerformer(DiscardCardsGA discardCardsGA)
    {
        if (discardCardsGA.Amount <= 0)
            yield break;

        int actualDiscardCount = Mathf.Min(discardCardsGA.Amount, hand.Count);

        for (int i = 0; i < actualDiscardCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, hand.Count);
            Card card = hand[randomIndex];

            hand.RemoveAt(randomIndex);
            discardPile.Add(card);

            CombatDomainEventBus.Publish(
                new CardDiscardedFromHandEvent(card.RuntimeId)
            );

            UpdatePileCounts();
            yield return null;
        }
    }

    private void RefillDeck()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        drawPile.Shuffle();

        UpdatePileCounts();
    }

    private IEnumerator DrawCard()
    {
        if (drawPile.Count == 0)
            yield break;

        Card card = drawPile.Draw();
        hand.Add(card);

        CombatDomainEventBus.Publish(
            new CardDrawnToHandEvent(card)
        );

        var limit = CombatContextService.Instance.Hero?.MaxHandSize ?? MaxHandSizeSafty;
        if (hand.Count > limit)
        {
            hand.Remove(card);
            discardPile.Add(card);

            CombatDomainEventBus.Publish(
                new CardDiscardedFromHandEvent(card.RuntimeId)
            );
        }

        UpdatePileCounts();
        yield return null;
    }

    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        if (hand.Count == 0)
        {
            UpdatePileCounts();
            yield break;
        }

        var cardsToDiscard = new List<Card>(hand);

        foreach (var card in cardsToDiscard)
        {
            hand.Remove(card);
            discardPile.Add(card);

            CombatDomainEventBus.Publish(
                new CardDiscardedFromHandEvent(card.RuntimeId)
            );

            UpdatePileCounts();
            yield return null;
        }
    }

    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        Log.Info(LogArea.Combat, () =>
            $"PlayCardPerformer: {playCardGA.Card?.Title} Mana={playCardGA.Card?.Mana} ManualTarget={(playCardGA.ManualTargetId.HasValue)} CasterId={playCardGA.CasterId.Value} RuntimeId={playCardGA.Card?.RuntimeId}",
            this);

        var canCommit = CardPlayabilityService.Instance.EvaluateCommit(
            playCardGA.Card,
            playCardGA.CasterId,
            playCardGA.ManualTargetId
        );

        if (!canCommit.CanPlay)
            yield break;

        RoundStateSystem.Instance.NotifyCardPlayed(playCardGA.Card);

        hand.Remove(playCardGA.Card);
        discardPile.Add(playCardGA.Card);

        int sequenceId = CombatSequence.Next();

        CombatDomainEventBus.Publish(
            new CardPlayedEvent(
                sequenceId,
                playCardGA.CasterId,
                playCardGA.Card.RuntimeId,
                playCardGA.ManualTargetId
            )
        );

        yield return PresentationGate.Wait(sequenceId);

        SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
        ActionSystem.Instance.AddReaction(spendManaGA);

        Log.Debug(LogArea.Combat, () => "Added SpendManaGA", this);

        if (playCardGA.Card.HasManualTargetEffects)
        {
            if (!playCardGA.ManualTargetId.HasValue)
                yield break;

            var targetIds = new List<CombatantId> { playCardGA.ManualTargetId.Value };
            var casterId = (CombatantId?)playCardGA.CasterId;

            foreach (var effect in playCardGA.Card.ManualTargetEffects)
            {
                if (effect == null)
                    continue;

                ActionSystem.Instance.AddReaction(new PerformEffectsGA(effect, targetIds, casterId));
            }
        }

        var otherEffects = playCardGA.Card.OtherEffects;
        if (otherEffects != null)
        {
            var casterId = (CombatantId?)playCardGA.CasterId;

            foreach (var effectWrapper in otherEffects)
            {
                var targetIds = effectWrapper.TargetMode.GetTargetIds();
                ActionSystem.Instance.AddReaction(new PerformEffectsGA(effectWrapper.Effect, targetIds, casterId));
            }
        }

        UpdatePileCounts();
    }
}
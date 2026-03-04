using DG.Tweening;
using Game.Logging;
using Game.Scenes.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

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
        if (handView == null)
        {
            Log.Error(LogArea.Combat, () => "HandView reference is missing (handView). Assign it in the inspector.", this);
            return;
        }
        if (drawPilePoint == null)
        {
            Log.Error(LogArea.Combat, () => "drawPilePoint reference is missing. Assign it in the inspector.", this);
            return;
        }
        if (discardPilePoint == null)
        {
            Log.Error(LogArea.Combat, () => "discardPilePoint reference is missing. Assign it in the inspector.", this);
            return;
        }
        if (CardViewCreator.Instance == null)
        {
            Log.Error(LogArea.Combat, () => "CardViewCreator.Instance is null. Ensure a CardViewCreator exists in the Combat scene.", this);
            return;
        }

        foreach (var cardData in startingDeck)
        {
            Card card = new Card(cardData);
            drawPile.Add(card);
        }
        drawPile.Shuffle();
    }

    // Performers

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

            CardView cardView = handView.RemoveCard(card);
            if (cardView != null)
            {
                yield return DiscardCard(cardView);
            }
            else
            {
                // Safety
                UpdatePileCounts();
            }
        }

        UpdatePileCounts();
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
        Card card = drawPile.Draw();
        hand.Add(card);

        CardView cardView = CardViewCreator.Instance.CreateCardView(
            card,
            drawPilePoint.position,
            drawPilePoint.rotation,
            handView.HandCardContrainer.transform
        );

        yield return handView.AddCard(cardView);

        var limit = CombatContextService.Instance.Hero?.MaxHandSize ?? MaxHandSizeSafty;
        if (hand.Count > limit)
        {
            hand.Remove(cardView.Card);
            handView.RemoveCard(cardView.Card);

            yield return DiscardCard(cardView);
        }

        UpdatePileCounts();
    }

    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        foreach (var card in hand)
        {
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        hand.Clear();
        UpdatePileCounts();
    }

    private IEnumerator DiscardCard(CardView cardView)
    {
        discardPile.Add(cardView.Card);
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);

        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);

        UpdatePileCounts();
    }

    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        Log.Info(LogArea.Combat, () =>
            $"PlayCardPerformer: {playCardGA.Card?.Title} Mana={playCardGA.Card?.Mana} ManualTarget={(playCardGA.ManualTargetId.HasValue)} CasterId={playCardGA.CasterId.Value}",
            this);

        // Validate centrally so illegal plays can't slip through UI checks
        // CardPlayabilitySystem is still View-based -> resolve manual target view from id.
        EnemyView manualTargetView = null;
        if (playCardGA.ManualTargetId.HasValue)
            manualTargetView = FindEnemyViewById(playCardGA.ManualTargetId.Value);

        var canCommit = CardPlayabilityService.Instance.EvaluateCommit(playCardGA.Card, CombatPresentationController.Instance.HeroView, manualTargetView);
        if (!canCommit.CanPlay)
            yield break;

        RoundStateSystem.Instance.NotifyCardPlayed(playCardGA.Card);


        hand.Remove(playCardGA.Card);
        var cardView = handView.RemoveCard(playCardGA.Card);

        // logisch schon mal in discard
        discardPile.Add(playCardGA.Card);

        // Presentation gate: wait until card animation finished
        int token = PresentationGate.NewToken();
        CombatEventBus.Publish(new CardPlayPresentationRequestedEvent(cardView, discardPilePoint.position, token));
        yield return PresentationGate.Wait(token);
        //hand.Remove(playCardGA.Card);
        //CardView cardView = handView.RemoveCard(playCardGA.Card);
        //yield return DiscardCard(cardView);

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
                if (effect == null) continue;
                ActionSystem.Instance.AddReaction(new PerformEffectsGA(effect, targetIds, casterId));
            }
        }

        var otherEffects = playCardGA.Card.OtherEffects;
        if (otherEffects != null)
        {
            var casterId = (CombatantId?)playCardGA.CasterId;

            foreach (var effectWrapper in otherEffects)
            {
                var targetIds = effectWrapper.TargetMode.GetTargetIds(); // siehe TargetMode-Teil unten
                ActionSystem.Instance.AddReaction(new PerformEffectsGA(effectWrapper.Effect, targetIds, casterId));
            }
        }

        UpdatePileCounts();
    }

    private EnemyView FindEnemyViewById(CombatantId id)
    {
        var pc = CombatPresentationController.Instance;
        if (pc == null) return null;

        return pc.TryGetEnemyView(id, out var view) ? view : null;
    }
}
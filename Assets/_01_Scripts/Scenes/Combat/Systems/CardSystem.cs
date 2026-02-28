using DG.Tweening;
using Game.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;
    public int MaxHandSize = 10;

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
        {
            yield return DrawCard();
        }

        if (notEnoughCards > 0)
        {
            RefillDeck();
            for (int i = 0; i < notEnoughCards; i++)
            {
                yield return DrawCard();
            }
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
                //Safty?
                UpdatePileCounts(); 
            }
        }

        UpdatePileCounts();
    }

    private void RefillDeck()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        //discardPilePoint.ClearView();
        drawPile.Shuffle();

        UpdatePileCounts();
    }

    private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        hand.Add(card);

        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        yield return handView.AddCard(cardView);

        if (hand.Count > MaxHandSize)
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
        Debug.Log($"[CardSystem] PlayCardPerformer: {playCardGA.Card?.Title} Mana={playCardGA.Card?.Mana} ManualTarget={(playCardGA.ManualTarget != null)} Caster={(playCardGA.Caster != null)}");

        // Validate centrally so illegal plays can't slip through UI checks
        var canCommit = CardPlayabilitySystem.Instance.EvaluateCommit(playCardGA.Card, HeroSystem.Instance.HeroView, playCardGA.ManualTarget);
        if (!canCommit.CanPlay)
            yield break;

        RoundStateSystem.Instance.NotifyCardPlayed(playCardGA.Card);

        hand.Remove(playCardGA.Card);
        CardView cardView = handView.RemoveCard(playCardGA.Card);
        yield return DiscardCard(cardView);

        SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
        ActionSystem.Instance.AddReaction(spendManaGA);
        Debug.Log("[CardSystem] Added SpendManaGA");

        if (playCardGA.Card.HasManualTargetEffects)
        {
            Log.Debug(LogCat.Gameplay, () =>
                $"[CardSystem] ManualTargetEffects={playCardGA.Card.ManualTargetEffects?.Count ?? 0}, " +
                $"OtherEffects={(playCardGA.Card.OtherEffects?.Count ?? 0)}");

            if (playCardGA.ManualTarget == null)
            {
                Debug.LogWarning("[CardSystem] Card has ManualTargetEffects but ManualTarget is null. Skipping.");
            }
            else
            {
                var targets = new List<CombatantView> { playCardGA.ManualTarget };

                foreach (var effect in playCardGA.Card.ManualTargetEffects)
                {
                    if (effect == null) continue;

                    var performEffectsGA = new PerformEffectsGA(effect, targets, playCardGA.Caster);
                    ActionSystem.Instance.AddReaction(performEffectsGA);
                }
            }
        }


        var otherEffects = playCardGA.Card.OtherEffects;
        if (otherEffects != null)
        { 
            foreach (var effectWrapper in otherEffects)
            {
                List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
                PerformEffectsGA performEffectsGA = new(effectWrapper.Effect, targets, playCardGA.Caster);
                ActionSystem.Instance.AddReaction(performEffectsGA);
            }
        }

        UpdatePileCounts();

    }
}
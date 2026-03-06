using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Card
{
    private static long nextRuntimeId = 0;

    private readonly CardData cardData;
    public CardData Data => cardData;

    public long RuntimeId { get; }

    public string Title => string.IsNullOrWhiteSpace(cardData.DisplayName) ? cardData.name : cardData.DisplayName;
    public string Description => CardTextFormattingService.Instance.Format(cardData.Description);
    public CardRarity Rarity => cardData.Rarity;

    public int Mana { get; private set; }
    public Sprite Image => cardData.Image;

    public IReadOnlyList<Effect> ManualTargetEffects => cardData.ManualTargetEffects;
    public IReadOnlyList<AutoTargetEffect> OtherEffects => cardData.OtherEffects;
    public IReadOnlyList<CardTag> Tags => cardData.Tags;

    public bool HasTag(CardTag tag) => cardData.Tags != null && cardData.Tags.Contains(tag);

    public IReadOnlyList<CardCondition> PlayConditions => cardData.PlayConditions;

    public bool HasManualTargetEffects =>
        cardData.ManualTargetEffects != null && cardData.ManualTargetEffects.Count > 0;

    public Card(CardData data)
    {
        cardData = data;
        RuntimeId = Interlocked.Increment(ref nextRuntimeId);
        Mana = data.Mana;
    }
}
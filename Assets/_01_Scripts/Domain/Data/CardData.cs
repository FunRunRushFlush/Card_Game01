using SerializeReferenceEditor;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [Header("Stable ID")]
    [field: SerializeField] public string Id;

    [field: SerializeField] public string DisplayName;
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Mana { get; private set; }

    [field: SerializeField] public Sprite Image { get; private set; }

    [Header("LootProperty")]
    [field: SerializeField] public CardRarity Rarity = CardRarity.Common;

    [field: SerializeField] public BiomeType[] PreferredBiomes;
    [field: SerializeField] public List<CardTag> Tags { get; private set; } = new();

    [Header("Card Effects")]
    [field: SerializeReference, SR] public List<Effect> ManualTargetEffects { get; private set; } = new();

    [field: SerializeReference, SR] public List<AutoTargetEffect> OtherEffects { get; private set; }

    [field: SerializeReference, SR] public List<CardCondition> PlayConditions { get; private set; } = new();

}



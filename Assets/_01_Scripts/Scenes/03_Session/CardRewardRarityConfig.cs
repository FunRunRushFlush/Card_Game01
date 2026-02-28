using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card Reward Rarity Config")]
public class CardRewardRarityConfig : ScriptableObject
{
    [Header("General")]
    public int ChoiceCount = 3;
    public float BiomeBoost = 3f;
    public bool IncludeAdminAndStarter = false;

    [Header("Weights per Encounter Tier")]
    public Entry[] Normal = new Entry[]
    {
        new Entry(CardRarity.Common, 70f),
        new Entry(CardRarity.Uncommon, 21f),
        new Entry(CardRarity.Rare, 7f),
        new Entry(CardRarity.Epic, 2f),
        new Entry(CardRarity.Admin, 0f),
        new Entry(CardRarity.Starter, 0f),
    };

    public Entry[] Elite = new Entry[]
    {
        new Entry(CardRarity.Common, 70f),
        new Entry(CardRarity.Uncommon, 21f),
        new Entry(CardRarity.Rare, 7f),
        new Entry(CardRarity.Epic, 2f),
        new Entry(CardRarity.Admin, 0f),
        new Entry(CardRarity.Starter, 0f),
    };

    public Entry[] Boss = new Entry[]
    {
        new Entry(CardRarity.Rare, 80f),
        new Entry(CardRarity.Epic, 20f),
        new Entry(CardRarity.Admin, 0f),
        new Entry(CardRarity.Starter, 0f),
        new Entry(CardRarity.Common, 0f),
        new Entry(CardRarity.Uncommon, 0f),
    };


    [Serializable]
    public struct Entry
    {
        public CardRarity Rarity;
        public float Weight;

        public Entry(CardRarity rarity, float weight)
        {
            Rarity = rarity;
            Weight = weight;
        }
    }

    public IReadOnlyList<RarityWeight> GetWeights(EncounterTier tier)
    {
        Entry[] src = tier switch
        {
            EncounterTier.Elite => Elite,
            EncounterTier.Boss => Boss,
            _ => Normal,
        };

        var list = new List<RarityWeight>(src != null ? src.Length : 0);
        if (src != null)
        {
            for (int i = 0; i < src.Length; i++)
                list.Add(new RarityWeight(src[i].Rarity, src[i].Weight));
        }

        return list;
    }


}

using System;
using System.Collections.Generic;
using System.Linq;

public static class CardRewardGenerator
{
    public static List<CardData> GenerateChoices(
        IReadOnlyList<CardData> pool,
        IReadOnlyList<CardData> deck,
        BiomeType biomeType,
        System.Random rng,
        IReadOnlyList<RarityWeight> rarityWeights,
        int choiceCount = 3,
        float biomeBoost = 3f,
        bool includeAdminAndStarter = false)
    {


        // Base pool: null + (Admin/Starter optional) raus
        var basePool = (pool ?? Array.Empty<CardData>())
            .Where(c => c != null)
            .Where(c => includeAdminAndStarter || (c.Rarity != CardRarity.Admin && c.Rarity != CardRarity.Starter))
            .ToList();

        var choices = new List<CardData>(choiceCount);
        if (basePool.Count == 0) return choices;

        // Group by rarity for faster draws
        var byRarity = basePool
            .GroupBy(c => c.Rarity)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<CardData>)g.ToList());

        for (int i = 0; i < choiceCount; i++)
        {
            var rarity = RollRarity(rarityWeights, byRarity, rng);

            // if no cards exist for rolled rarity -> fallback to basePool
            if (!byRarity.TryGetValue(rarity, out var drawPool) || drawPool == null || drawPool.Count == 0)
                drawPool = basePool;

            var pick = PickWeighted(
                drawPool,
                alreadyChosen: choices,
                biome: biomeType,
                rng: rng,
                biomeBoost: biomeBoost);

            if (pick != null)
                choices.Add(pick);
        }

        return choices;
    }

    private static CardRarity RollRarity(
        IReadOnlyList<RarityWeight> weights,
        Dictionary<CardRarity, IReadOnlyList<CardData>> availableByRarity,
        System.Random rng)
    {
        // Only consider rarities that exist in pool AND weight > 0
        float total = 0f;
        var tmp = new List<RarityWeight>(weights.Count);

        for (int i = 0; i < weights.Count; i++)
        {
            var w = weights[i];
            if (w.Weight <= 0f) continue;

            if (availableByRarity != null &&
                availableByRarity.TryGetValue(w.Rarity, out var list) &&
                list != null && list.Count > 0)
            {
                tmp.Add(w);
                total += w.Weight;
            }
        }

        // fallback: if nothing valid -> just pick any rarity that exists
        if (tmp.Count == 0)
        {
            if (availableByRarity != null && availableByRarity.Count > 0)
                return availableByRarity.Keys.First();
            return CardRarity.Common;
        }

        float roll = (float)(rng.NextDouble() * total);
        for (int i = 0; i < tmp.Count; i++)
        {
            roll -= tmp[i].Weight;
            if (roll <= 0f)
                return tmp[i].Rarity;
        }

        return tmp[tmp.Count - 1].Rarity;
    }

  

    private static bool IsPreferredForBiome(CardData card, BiomeType biome)
    {
        if (card.PreferredBiomes == null || card.PreferredBiomes.Length == 0)
            return false;

        for (int i = 0; i < card.PreferredBiomes.Length; i++)
            if (card.PreferredBiomes[i].Equals(biome))
                return true;

        return false;
    }

    private static CardData PickWeighted(
        IReadOnlyList<CardData> pool,
        List<CardData> alreadyChosen,
        BiomeType biome,
        System.Random rng,
        float biomeBoost = 3f)
    {
        float total = 0f;
        var weights = new float[pool.Count];

        for (int i = 0; i < pool.Count; i++)
        {
            var c = pool[i];
            if (c == null) { weights[i] = 0f; continue; }

            // avoid duplicates inside the same reward set
            if (alreadyChosen.Contains(c)) { weights[i] = 0f; continue; }

            float w = 1f;

            if (IsPreferredForBiome(c, biome))
                w *= biomeBoost;

            weights[i] = w;
            total += w;
        }

        if (total <= 0f)
        {
            // fallback: random from pool (allow duplicates)
            return pool.Count > 0 ? pool[rng.Next(0, pool.Count)] : null;
        }

        float roll = (float)(rng.NextDouble() * total);
        for (int i = 0; i < pool.Count; i++)
        {
            roll -= weights[i];
            if (roll <= 0f)
                return pool[i];
        }

        return pool[pool.Count - 1];
    }
}

[Serializable]
public readonly struct RarityWeight
{
    public readonly CardRarity Rarity;
    public readonly float Weight;

    public RarityWeight(CardRarity rarity, float weight)
    {
        Rarity = rarity;
        Weight = weight;
    }
}

using Game.Logging;
using Game.Scenes.Core;
using System.Collections.Generic;
using UnityEngine;

public class RewardSystem : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private CardRewardRarityConfig cardRewardRarityConfig;

    private bool _isValid;

    private void Awake()
    {
        _isValid = ValidateConfig(cardRewardRarityConfig);

        if (!_isValid)
        {
            // Hard fail: System deaktivieren, damit es nicht “halb” läuft
            enabled = false;
            return;
        }

        LogConfigSummary(cardRewardRarityConfig);
    }

    public List<CardData> GenerateCardChoices()
    {
        if (!_isValid)
            return new List<CardData>();

        var session = CoreManager.Instance?.Session;
        if (session == null || session.Run == null || session.CardDatabase == null || session.Hero == null)
            return new List<CardData>();

        var run = session.Run;
        var ctx = run.CurrentRewardContext;
        var biom = run.CurrentRewardContext.Biome;

        var choiceCount = cardRewardRarityConfig.ChoiceCount;
        var biomeBoost = cardRewardRarityConfig.BiomeBoost;
        var weights = cardRewardRarityConfig.GetWeights(ctx.Tier);
        var includeSpecial = cardRewardRarityConfig.IncludeAdminAndStarter;

        var rng = run.CreateNodeRng(salt: 9001);

        return CardRewardGenerator.GenerateChoices(
            pool: session.CardDatabase.AllCards,
            deck: session.Hero.Deck,
            biomeType: biom,
            rng: rng,
            choiceCount: choiceCount,
            biomeBoost: biomeBoost,
            rarityWeights: weights,
            includeAdminAndStarter: includeSpecial);
    }


    public List<CardData> GenerateCardChoicesForBiome(BiomeType biome, float biomeBoostFactor = 999f)
    {
        if (!_isValid)
            return new List<CardData>();

        var session = CoreManager.Instance?.Session;
        if (session == null || session.Run == null || session.CardDatabase == null || session.Hero == null)
            return new List<CardData>();

        var run = session.Run;
        var ctx = run.CurrentRewardContext;

        var biom = biome;
        var biomeBoost = biomeBoostFactor;

        var choiceCount = cardRewardRarityConfig.ChoiceCount;
        var weights = cardRewardRarityConfig.GetWeights(ctx.Tier);
        var includeSpecial = cardRewardRarityConfig.IncludeAdminAndStarter;

        var rng = run.CreateNodeRng(salt: 9001);

        return CardRewardGenerator.GenerateChoices(
            pool: session.CardDatabase.AllCards,
            deck: session.Hero.Deck,
            biomeType: biom,
            rng: rng,
            choiceCount: choiceCount,
            biomeBoost: biomeBoost,
            rarityWeights: weights,
            includeAdminAndStarter: includeSpecial);
    }

    public int GenerateGold(int minInclusive, int maxExclusive)
    {
        if (!_isValid)
            return 0;

        var session = CoreManager.Instance?.Session;
        if (session?.Run == null)
            return 0;

        var rng = session.Run.CreateNodeRng(salt: 9002);
        return rng.Next(minInclusive, maxExclusive);
    }

    private static bool ValidateConfig(CardRewardRarityConfig cfg)
    {
        if (cfg == null)
        {
            Log.Error(LogCat.General, () => "[RewardSystem] Missing CardRewardRarityConfig reference.");
            return false;
        }

        bool ok = true;

        if (cfg.ChoiceCount <= 0)
        {
            Log.Error(LogCat.General, () => $"[RewardSystem] ChoiceCount must be > 0 but was {cfg.ChoiceCount}.");
            ok = false;
        }

        if (cfg.BiomeBoost < 0f)
        {
            Log.Error(LogCat.General, () => $"[RewardSystem] BiomeBoost must be >= 0 but was {cfg.BiomeBoost}.");
            ok = false;
        }

        ok &= ValidateTier("Normal", cfg.Normal);
        ok &= ValidateTier("Elite", cfg.Elite);
        ok &= ValidateTier("Boss", cfg.Boss);

        return ok;
    }

    private static bool ValidateTier(string name, CardRewardRarityConfig.Entry[] entries)
    {
        if (entries == null || entries.Length == 0)
        {
            Log.Error(LogCat.General, () => $"[RewardSystem] {name} weights are null/empty.");
            return false;
        }

        float sum = 0f;
        for (int i = 0; i < entries.Length; i++)
        {
            var w = entries[i].Weight;

            if (float.IsNaN(w) || float.IsInfinity(w))
            {
                Log.Error(LogCat.General, () => $"[RewardSystem] {name} has invalid weight (NaN/Inf) at index {i}.");
                return false;
            }

            if (w < 0f)
            {
                Log.Error(LogCat.General, () => $"[RewardSystem] {name} has negative weight {w} at index {i}.");
                return false;
            }

            sum += w;
        }

        if (sum <= 0f)
        {
            Log.Error(LogCat.General, () => $"[RewardSystem] {name} total weight must be > 0 but was {sum}.");
            return false;
        }

        return true;
    }

    private static void LogConfigSummary(CardRewardRarityConfig cfg)
    {
        Log.Info(LogCat.General, () =>
            $"[RewardSystem] CardRewardConfig OK | ChoiceCount={cfg.ChoiceCount}, BiomeBoost={cfg.BiomeBoost}, IncludeAdminAndStarter={cfg.IncludeAdminAndStarter}");
    }
}

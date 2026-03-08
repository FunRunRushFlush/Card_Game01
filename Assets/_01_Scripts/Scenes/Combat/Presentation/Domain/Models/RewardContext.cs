public enum EncounterTier { Normal, Elite, Boss }

[System.Serializable]
public struct RewardContext
{
    public EncounterTier Tier;
    public BiomeType Biome;
}

using Game.Scenes.Core;
using System;
using UnityEngine;

public class RunState : MonoBehaviour
{
    [Header("Run Content")]
    [SerializeField] private RunMapDefinition mapDefinition;

    public RunMapDefinition MapDefinition => mapDefinition;

    // --- Progress ---
    public int BiomeIndex { get; private set; } = 0;
    public int NodeIndexInBiome { get; private set; } = 0;

    // Optional: globaler NodeIndex für Stats/Seed/Anzeige
    public int GlobalNodeIndex { get; private set; } = 0;

    public int Gold { get; private set; }
    public float RunStartTime { get; private set; }

    public RewardContext CurrentRewardContext { get; private set; }
    public bool HasRewardContext { get; private set; }

    public event Action<int> GoldChanged;
    public event Action<int> NodeChanged;

    public int RunSeed { get; private set; }

    // --- Current Node helpers ---
    public bool HasValidMap =>
        mapDefinition != null &&
        mapDefinition.biomes != null &&
        mapDefinition.biomes.Count > 0;

    public BiomeMapDefinition CurrentBiomeMap
    {
        get
        {
            if (!HasValidMap) return null;
            int i = Mathf.Clamp(BiomeIndex, 0, mapDefinition.biomes.Count - 1);
            return mapDefinition.biomes[i];
        }
    }

    public BiomeType CurrentBiome
    {
        get
        {
            var biomeMap = CurrentBiomeMap;
            return biomeMap != null ? biomeMap.biome : default;
        }
    }

    public MapNodeDefinition CurrentNode
    {
        get
        {
            var biomeMap = CurrentBiomeMap;
            if (biomeMap == null || biomeMap.nodes == null || biomeMap.nodes.Count == 0)
                return null;

            int i = Mathf.Clamp(NodeIndexInBiome, 0, biomeMap.nodes.Count - 1);
            return biomeMap.nodes[i];
        }
    }

    public MapNodeType CurrentNodeType =>
        CurrentNode != null ? CurrentNode.type : MapNodeType.Combat;

    public bool IsBossNode => CurrentNodeType == MapNodeType.Boss;

    public bool IsFinalBiome =>
        HasValidMap && BiomeIndex >= mapDefinition.biomes.Count - 1;

    public bool IsFinalBossNode => IsFinalBiome && IsBossNode;

    public void StartNewRun(int startingGold = 0)
    {
        if (!HasValidMap)
        {
            Debug.LogError("[RunState] Missing/invalid RunMapDefinition. Please assign one in the inspector.");
            // trotzdem initialisieren, damit nix crasht
        }

        RunSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        Gold = startingGold;
        RunStartTime = Time.unscaledTime;

        BiomeIndex = 0;
        NodeIndexInBiome = 0;
        GlobalNodeIndex = 0;

        HasRewardContext = false;

        GoldChanged?.Invoke(Gold);
        NodeChanged?.Invoke(GlobalNodeIndex);
    }

    public void ChangeAmountOfGold(int amount)
    {
        Gold = Mathf.Max(0, Gold + amount);
        GoldChanged?.Invoke(Gold);
    }

    public void QuitRun()
    {
        GameFlowController.Current.BackToMainMenu();
    }

    public int GetNodeSeed(int salt = 1337)
    {
        unchecked
        {
            return RunSeed ^ (BiomeIndex * 1_000_003) ^ (NodeIndexInBiome * 9_173) ^ salt;
        }
    }

    public System.Random CreateNodeRng(int salt = 1337) => new System.Random(GetNodeSeed(salt));

    public void AdvanceNode()
    {
        GlobalNodeIndex++;

        if (!HasValidMap)
        {
            NodeIndexInBiome++;
            NodeChanged?.Invoke(GlobalNodeIndex);
            return;
        }

        var biomeMap = CurrentBiomeMap;
        int nodeCount = (biomeMap != null && biomeMap.nodes != null) ? biomeMap.nodes.Count : 0;

        NodeIndexInBiome++;

        // Biome fertig? -> nächstes Biome, NodeIndex reset
        if (nodeCount > 0 && NodeIndexInBiome >= nodeCount)
        {
            NodeIndexInBiome = 0;
            BiomeIndex++;
        }

        NodeChanged?.Invoke(GlobalNodeIndex);
    }

    public void SetRewardContext(RewardContext ctx)
    {
        CurrentRewardContext = ctx;
        HasRewardContext = true;
    }

    public float GetRunTimeSeconds() => Time.unscaledTime - RunStartTime;
}

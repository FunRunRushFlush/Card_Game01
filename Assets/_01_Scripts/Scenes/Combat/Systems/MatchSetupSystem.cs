using System;
using Game.Logging;
using Game.Scenes.Core;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private BiomeDatabase biomeDb;
    [SerializeField] private PerkData perkData;

    [Header("Bootstrap")]
    [SerializeField] private CombatBootstrapper combatBootstrapper;

    void Start()
    {

        if (CombatSandboxMode.IsActive)
        {
            Debug.Log("[MatchSetupSystem] Sandbox mode active -> skipping normal run combat setup.");
            return;
        }


        var session = CoreManager.Instance.Session;
        var run = session.Run;

        var biomeDef = biomeDb.Get(run.CurrentBiome);
        if (biomeDef == null)
        {
            Log.Error(LogArea.General, () => $"No BiomeDefinition found for biome {run.CurrentBiome}");
            return;
        }

        var nodeType = run.CurrentNodeType;

        // Safety: Combat scene should not be entered for Shop/Event nodes
        if (nodeType == MapNodeType.Shop || nodeType == MapNodeType.Event)
        {
            Log.Error(LogArea.General, () => $"Entered Combat scene on non-combat node: {nodeType} (Biome={run.CurrentBiome}, Node={run.NodeIndexInBiome})");
            return;
        }

        if (combatBootstrapper == null)
        {
            Log.Error(LogArea.General, () => "CombatBootstrapper is missing on MatchSetupSystem. Assign it in the inspector.");
            return;
        }

        EncounterDefinition encounterDef = null;

        switch (nodeType)
        {
            case MapNodeType.Boss:
                // Optional override on the node, otherwise biome boss
                encounterDef = (run.CurrentNode != null && run.CurrentNode.bossOverride != null)
                    ? run.CurrentNode.bossOverride
                    : biomeDef.bossEncounter;
                break;

            case MapNodeType.EliteCombat:
                {
                    // Optional override on the node
                    if (run.CurrentNode != null && run.CurrentNode.combatOverride != null)
                    {
                        encounterDef = run.CurrentNode.combatOverride;
                        break;
                    }

                    // Pick from elite pool (seeded)
                    if (biomeDef.eliteEncounters != null && biomeDef.eliteEncounters.Length > 0)
                    {
                        var elitePickRng = run.CreateNodeRng(salt: 777);
                        encounterDef = biomeDef.eliteEncounters[elitePickRng.Next(0, biomeDef.eliteEncounters.Length)];
                        break;
                    }

                    // Fallback to normal encounters if elite pool missing
                    if (biomeDef.nodeEncounters != null && biomeDef.nodeEncounters.Length > 0)
                    {
                        int idx = Mathf.Clamp(run.NodeIndexInBiome, 0, biomeDef.nodeEncounters.Length - 1);
                        encounterDef = biomeDef.nodeEncounters[idx];
                        break;
                    }

                    Log.Error(LogArea.General, () => $"Biome '{run.CurrentBiome}' has no eliteEncounters and no nodeEncounters configured.");
                    return;
                }

            default: // MapNodeType.Combat
                {
                    // Optional override on the node
                    if (run.CurrentNode != null && run.CurrentNode.combatOverride != null)
                    {
                        encounterDef = run.CurrentNode.combatOverride;
                        break;
                    }

                    if (biomeDef.nodeEncounters == null || biomeDef.nodeEncounters.Length == 0)
                    {
                        Log.Error(LogArea.General, () => $"Biome '{run.CurrentBiome}' has no nodeEncounters configured.");
                        return;
                    }

                    int idx = Mathf.Clamp(run.NodeIndexInBiome, 0, biomeDef.nodeEncounters.Length - 1);
                    encounterDef = biomeDef.nodeEncounters[idx];
                    break;
                }
        }

        if (encounterDef == null)
        {
            Debug.LogError($"EncounterDefinition missing. Biome={run.CurrentBiome}, nodeInBiome={run.NodeIndexInBiome}, nodeType={nodeType}");
            return;
        }

        // Deterministic per run + node
        int seed = run.GetNodeSeed(salt: 1337);
        var rng = new System.Random(seed);

        // Resolve enemies (this is what we want to freeze into the snapshot)
        var enemies = EncounterResolver.Resolve(encounterDef, rng);

        // Store reward context for the loot scene (unchanged)
        run.SetRewardContext(new RewardContext
        {
            Tier = nodeType switch
            {
                MapNodeType.Boss => EncounterTier.Boss,
                MapNodeType.EliteCombat => EncounterTier.Elite,
                _ => EncounterTier.Normal
            },
            Biome = run.CurrentBiome
        });

        // Build snapshot AFTER resolve so it's 100% reproducible even if encounter pools change later
        var snapshot = new CombatSetupSnapshot
        {
            createdAtUtc = DateTime.UtcNow.ToString("O"),
            seed = seed,

            heroId = (int)session.Hero.Data.HeroID,
            handSize = session.Hero.Data.DrawPerTurn,

            biome = run.CurrentBiome.ToString(),
            nodeType = nodeType.ToString(),
            biomeIndex = run.BiomeIndex,
            nodeIndexInBiome = run.NodeIndexInBiome,

            encounterId = encounterDef.Id
        };

        // Freeze resolved enemies
        foreach (var e in enemies)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.Id))
                Debug.LogError("[CombatSnapshot] EnemyData has missing Id. Please set it.");
            else
                snapshot.enemyIds.Add(e.Id);
        }

        // Freeze deck snapshot (IDs)
        var deckCards = session.Hero.CreateCombatSnapshot();
        foreach (var c in deckCards)
        {
            if (c == null || string.IsNullOrWhiteSpace(c.Id))
                Debug.LogError("[CombatSnapshot] CardData has missing Id. Please set it.");
            else
                snapshot.deckCardIds.Add(c.Id);
        }

        // Always save + log JSON + timestamped copy
        CombatSnapshotIO.SaveAlways(snapshot);

        // Start combat through the single entry point
        combatBootstrapper.StartCombat(snapshot);
    }
}
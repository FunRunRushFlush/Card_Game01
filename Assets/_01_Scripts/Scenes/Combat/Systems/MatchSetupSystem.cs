using Game.Logging;
using Game.Scenes.Core;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private BiomeDatabase biomeDb;
    [SerializeField] private PerkData perkData;

    void Start()
    {
        var session = CoreManager.Instance.Session;
        var run = session.Run;

        HeroSystem.Instance.Setup(session.Hero.Data);

        var biomeDef = biomeDb.Get(run.CurrentBiome);
        if (biomeDef == null)
        {
            Log.Error(LogCat.General, () => $"No BiomeDefinition found for biome {run.CurrentBiome}");
            return;
        }

        var nodeType = run.CurrentNodeType;

        // Safety: Combat scene should not be entered for Shop/Event nodes
        if (nodeType == MapNodeType.Shop || nodeType == MapNodeType.Event)
        {
            Log.Error(LogCat.General, () => $"Entered Combat scene on non-combat node: {nodeType} (Biome={run.CurrentBiome}, Node={run.NodeIndexInBiome})");
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

                    Log.Error(LogCat.General, () => $"Biome '{run.CurrentBiome}' has no eliteEncounters and no nodeEncounters configured.");
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
                        Log.Error(LogCat.General, () => $"Biome '{run.CurrentBiome}' has no nodeEncounters configured.");
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

        // deterministic per run + node
        var rng = run.CreateNodeRng(salt: 1337);

        var enemies = EncounterResolver.Resolve(encounterDef, rng);

        // store reward context for the loot scene
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

        EnemySystem.Instance.Setup(enemies);

        CardSystem.Instance.Setup(session.Hero.CreateCombatSnapshot());
        if (perkData != null) PerkSystem.Instance.AddPerk(new Perk(perkData));

        ActionSystem.Instance.Perform(new DrawCardsGA(session.Hero.Data.HandSize));
    }
}

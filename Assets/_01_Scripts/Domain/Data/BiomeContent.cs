using Game.Logging;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Biome Content")]

public class BiomeContent : ScriptableObject
{
    public BiomeType biome;

    [Header("Combat Content")]
    public EncounterTable normalCombat;
    public EncounterTable eliteCombat;
    public EncounterDefinition boss;

    [Header("Non-Combat Content")]
    public EventTable events;
    public ShopConfig shopConfig;

    [Header("Scenes")]
    public string[] normalArenaScenes;
    public string[] eliteArenaScenes;
    public string bossArenaScene;
}

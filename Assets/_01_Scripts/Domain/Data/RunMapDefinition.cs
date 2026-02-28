using System;
using System.Collections.Generic;
using UnityEngine;

public enum MapNodeType
{
    Combat,
    EliteCombat,
    Shop,
    Event,
    Boss
}

[Serializable]
public class MapNodeDefinition
{
    public MapNodeType type;

 
    public EncounterDefinition combatOverride;
    public EncounterDefinition bossOverride;


    [Header("Scene Overrides (optional)")]
    [Tooltip("Override for the *systems/view* scene loaded for this node (e.g. Shop UI scene or Event scene). Leave empty to use the default from SceneDatabase.")]
    public string sceneOverride;

    [Tooltip("Override for the *level/background* scene loaded for this node (used mainly by Shop/Combat). Leave empty to use biome/default arena selection.")]
    public string levelSceneOverride;
}

[Serializable]
public class BiomeMapDefinition
{
    public BiomeType biome;
    public List<MapNodeDefinition> nodes;
}

[CreateAssetMenu(menuName = "Run/Run Map Definition")]
public class RunMapDefinition : ScriptableObject
{
    public List<BiomeMapDefinition> biomes;
}

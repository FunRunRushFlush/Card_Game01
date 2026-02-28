using Game.Logging;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/BiomeDatabase")]
public class BiomeDatabase : ScriptableObject
{
    public List<BiomeDefinition> biomes;
    public string normalArenaScene;
    public string bossArenaScene;

    public BiomeDefinition Get(BiomeType type)
    {
        var def = biomes.Find(b => b.Biome == type);
        if (def == null)
        {
            Log.Error(LogCat.General, () => $"BiomeDefinition missing for {type}");
        }
        return def;
    }

}

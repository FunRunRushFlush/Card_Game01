using System.Collections.Generic;

public static class EncounterResolver
{
    public static List<EnemyData> Resolve(EncounterDefinition def, System.Random rng)
    {
        if (def == null || def.pool == null || def.pool.Count == 0)
            return new List<EnemyData>();

        int min = System.Math.Max(0, def.minEnemies);
        int max = System.Math.Max(min, def.maxEnemies);

        int count = rng.Next(min, max + 1);
        var result = new List<EnemyData>(count);

        for (int i = 0; i < count; i++)
            result.Add(PickWeighted(def.pool, rng));

        return result;
    }

    static EnemyData PickWeighted(List<WeightedEnemy> pool, System.Random rng)
    {
        int total = 0;
        foreach (var w in pool) total += w.weight;

        int roll = rng.Next(0, total);
        foreach (var w in pool)
        {
            roll -= w.weight;
            if (roll < 0) return w.data;
        }
        return pool[^1].data;
    }
}

using Game.Logging;
using System;
using System.Collections.Generic;

/// <summary>
/// Runtime lookup for combat content (enemies, encounters, heroes).
/// Keeps indexing and validation out of CombatBootstrapper.
/// </summary>
public sealed class CombatContentRegistry
{
    public IReadOnlyDictionary<string, EnemyData> EnemiesById => _enemiesById;
    public IReadOnlyDictionary<string, EncounterDefinition> EncountersById => _encountersById;
    public IReadOnlyDictionary<int, HeroData> HeroesById => _heroesById;

    private readonly Dictionary<string, EnemyData> _enemiesById;
    private readonly Dictionary<string, EncounterDefinition> _encountersById;
    private readonly Dictionary<int, HeroData> _heroesById;

    public CombatContentRegistry(CombatContentIndex index)
    {
        _enemiesById = BuildEnemyIndex(index);
        _encountersById = BuildEncounterIndex(index);
        _heroesById = BuildHeroIndex(index);

        Log.Info(LogArea.Combat, () =>
            $"[CombatContentRegistry] Index built. Enemies={_enemiesById.Count}, Encounters={_encountersById.Count}, Heroes={_heroesById.Count}");
    }

    public bool TryGetHero(int heroId, out HeroData hero)
        => _heroesById.TryGetValue(heroId, out hero) && hero != null;

    public bool TryGetEnemy(string enemyId, out EnemyData enemy)
        => _enemiesById.TryGetValue(enemyId, out enemy) && enemy != null;

    public bool TryGetEncounter(string encounterId, out EncounterDefinition enc)
        => _encountersById.TryGetValue(encounterId, out enc) && enc != null;

    private static Dictionary<string, EnemyData> BuildEnemyIndex(CombatContentIndex index)
    {
        var map = new Dictionary<string, EnemyData>(StringComparer.Ordinal);
        if (index == null) return map;

        foreach (var e in index.enemies)
        {
            if (e == null) continue;

            if (string.IsNullOrWhiteSpace(e.Id))
            {
                Log.Error(LogArea.Combat, () => $"[CombatContentRegistry] EnemyData '{e.name}' has empty Id.");
                continue;
            }

            if (!map.TryAdd(e.Id, e))
                Log.Warn(LogArea.Combat, () => $"[CombatContentRegistry] Duplicate EnemyData Id '{e.Id}' on '{e.name}'.");
        }

        return map;
    }

    private static Dictionary<string, EncounterDefinition> BuildEncounterIndex(CombatContentIndex index)
    {
        var map = new Dictionary<string, EncounterDefinition>(StringComparer.Ordinal);
        if (index == null) return map;

        foreach (var enc in index.encounters)
        {
            if (enc == null) continue;

            if (string.IsNullOrWhiteSpace(enc.Id))
            {
                Log.Error(LogArea.Combat, () => $"[CombatContentRegistry] EncounterDefinition '{enc.name}' has empty Id.");
                continue;
            }

            if (!map.TryAdd(enc.Id, enc))
                Log.Warn(LogArea.Combat, () => $"[CombatContentRegistry] Duplicate EncounterDefinition Id '{enc.Id}' on '{enc.name}'.");
        }

        return map;
    }

    private static Dictionary<int, HeroData> BuildHeroIndex(CombatContentIndex index)
    {
        var map = new Dictionary<int, HeroData>();
        if (index == null) return map;

        foreach (var h in index.heroes)
        {
            if (h == null) continue;

            var key = (int)h.HeroID;
            if (!map.TryAdd(key, h))
                Log.Warn(LogArea.Combat, () => $"[CombatContentRegistry] Duplicate HeroID '{h.HeroID}' on '{h.name}'.");
        }

        return map;
    }
}
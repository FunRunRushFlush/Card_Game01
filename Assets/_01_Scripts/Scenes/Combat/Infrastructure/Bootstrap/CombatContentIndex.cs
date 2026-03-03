using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Content Index")]
public class CombatContentIndex : ScriptableObject
{
    public List<EnemyData> enemies = new();
    public List<EncounterDefinition> encounters = new();
    public List<HeroData> heroes = new();
}
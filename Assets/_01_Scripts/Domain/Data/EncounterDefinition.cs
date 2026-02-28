using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Encounter")]
public class EncounterDefinition : ScriptableObject
{
    public int minEnemies = 1;
    public int maxEnemies = 3;
    public List<WeightedEnemy> pool;
}

[System.Serializable]
public struct WeightedEnemy
{
    public EnemyData data;
    public int weight;
}

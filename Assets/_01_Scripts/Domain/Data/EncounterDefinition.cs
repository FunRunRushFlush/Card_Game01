using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Encounter")]
public class EncounterDefinition : ScriptableObject
{
    [Header("Stable ID")]
    [field: SerializeField] private string id;
    public string Id => id;

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
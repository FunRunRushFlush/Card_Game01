using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EncounterTable")]
public class EncounterTable : ScriptableObject
{
    public List<EncounterDefinition> encountersPerNode;
    public EncounterDefinition bossEncounter;
}

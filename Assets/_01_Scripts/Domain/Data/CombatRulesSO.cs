using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Rules")]
public class CombatRulesSO : ScriptableObject
{
    [Header("Hand / Draw")]
    [Min(0)] public int defaultStartHandSize = 5;   // Fallback, wenn Snapshot/Hero nix liefert
    [Min(0)] public int drawPerTurn = 5;
    [Min(0)] public int maxHandSize = 10;

    [Header("Mana / Energy")]
    [Min(0)] public int startingMana = 3;

    // Optional für später:
    // public int defaultBlockDecay = 0;
}
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Discards a random number of cards from the hand.
/// TargetMode: NoTM
/// </summary>
public class DiscardRandomCardsEffect : Effect
{
    [SerializeField] private int amount = 1;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
        => new DiscardCardsGA(amount);
}
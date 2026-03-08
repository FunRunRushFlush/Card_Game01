using System.Collections.Generic;
using UnityEngine;

public class SpendComboPointsEffect : Effect
{
    [SerializeField] private int amount = 1;

    public override GameAction GetGameAction(IReadOnlyList<CombatantId> targets, CombatantId? caster)
        => new SpendComboPointsGA(amount);
}
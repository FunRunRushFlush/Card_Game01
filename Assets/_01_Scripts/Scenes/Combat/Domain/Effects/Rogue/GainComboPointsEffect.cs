using System.Collections.Generic;
using UnityEngine;

public class GainComboPointsEffect : Effect
{
    [SerializeField] private int amount = 1;

    public override GameAction GetGameAction(IReadOnlyList<CombatantId> targets, CombatantId? caster)
        => new GainComboPointsGA(amount);
}
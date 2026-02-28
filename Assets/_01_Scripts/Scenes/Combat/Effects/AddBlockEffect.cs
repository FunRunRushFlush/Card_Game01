using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that grants Block to the selected targets.
/// </summary>
public class AddBlockEffect : Effect
{
    [SerializeField] private int amount = 5;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        return new AddBlockGA(amount, targets, caster);
    }
}

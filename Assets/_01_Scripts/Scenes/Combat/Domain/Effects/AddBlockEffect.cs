using System.Collections.Generic;
using UnityEngine;

public class AddBlockEffect : Effect
{
    [SerializeField] private int amount = 5;

    public override GameAction GetGameAction(IReadOnlyList<CombatantId> targets, CombatantId? caster)
        => new AddBlockGA(amount, targets, caster);
}
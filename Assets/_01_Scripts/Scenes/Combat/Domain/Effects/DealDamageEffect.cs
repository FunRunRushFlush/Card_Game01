using System.Collections.Generic;
using UnityEngine;

public class DealDamageEffect : Effect
{
    [SerializeField] private int damageAmount;

    public override GameAction GetGameAction(IReadOnlyList<CombatantId> targets, CombatantId? caster)
        => new DealDamageGA(damageAmount, targets, caster);
}
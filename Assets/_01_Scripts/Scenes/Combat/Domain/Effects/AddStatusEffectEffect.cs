using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffectEffect : Effect
{
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private int stackCount;

    public override GameAction GetGameAction(IReadOnlyList<CombatantId> targets, CombatantId? caster)
        => new AddStatusEffectGA(statusEffectType, stackCount, targets);
}
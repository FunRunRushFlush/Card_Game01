using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffectEffect : Effect
{
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private int stackCount;


    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        var addStatusEffectGA = new AddStatusEffectGA(statusEffectType, stackCount, targets);
        return addStatusEffectGA;
    }
}

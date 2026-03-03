using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Removes status effect stacks from the target(s).
/// Typical use: remove WEAKNESS or POISON from the hero.
/// TargetMode: HeroTM
/// </summary>
public class CleanseEffect : Effect
{
    [SerializeField] private StatusEffectType statusEffectType = StatusEffectType.WEAKNESS;
    [SerializeField] private int stackCount = 1;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        return new RemoveStatusEffectGA(statusEffectType, stackCount, targets);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class RefillManaEffect : Effect
{
    public override GameAction GetGameAction(IReadOnlyList<CombatantId> targets, CombatantId? caster)
        => new RefillManaGA();
}
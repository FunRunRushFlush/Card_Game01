using System.Collections.Generic;

/// <summary>
/// Refills mana back to max.
/// </summary>
public class RefillManaEffect : Effect
{
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        return new RefillManaGA();
    }
}
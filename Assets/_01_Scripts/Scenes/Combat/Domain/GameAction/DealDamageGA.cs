using System.Collections.Generic;

public class DealDamageGA : GameAction, IHaveCaster
{
    public int Amount { get; }
    public IReadOnlyList<CombatantId> Targets { get; }
    public CombatantId? Caster { get; }

    public DealDamageGA(int amount, IReadOnlyList<CombatantId> targets, CombatantId? caster)
    {
        Amount = amount;
        Targets = targets;
        Caster = caster;
    }
}
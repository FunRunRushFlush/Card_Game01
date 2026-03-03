using System.Collections.Generic;

public class AddBlockGA : GameAction
{
    public int Amount { get; }
    public IReadOnlyList<CombatantId> Targets { get; }
    public CombatantId? Caster { get; }

    public AddBlockGA(int amount, IReadOnlyList<CombatantId> targets, CombatantId? caster)
    {
        Amount = amount;
        Targets = targets;
        Caster = caster;
    }
}
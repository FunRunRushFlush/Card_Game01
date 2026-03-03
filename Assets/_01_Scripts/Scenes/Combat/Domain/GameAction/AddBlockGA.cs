using System.Collections.Generic;


public class AddBlockGA : GameAction, IHaveCaster
{
    public int Amount { get; private set; }
    public List<CombatantView> Targets { get; private set; }
    public CombatantView Caster { get; private set; }

    public AddBlockGA(int amount, List<CombatantView> targets, CombatantView caster)
    {
        Amount = amount;
        Targets = new(targets);
        Caster = caster;
    }
}

using System.Collections.Generic;

public class AddStatusEffectGA : GameAction
{
    public StatusEffectType StatusEffectType { get; }
    public int StackCount { get; }
    public IReadOnlyList<CombatantId> Targets { get; }

    public AddStatusEffectGA(StatusEffectType type, int stacks, IReadOnlyList<CombatantId> targets)
    {
        StatusEffectType = type;
        StackCount = stacks;
        Targets = targets;
    }
}
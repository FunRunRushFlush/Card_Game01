using System.Collections.Generic;

public class PerformEffectsGA : GameAction
{
    public Effect Effect { get; }
    public IReadOnlyList<CombatantId> Targets { get; }
    public CombatantId? Caster { get; }

    public PerformEffectsGA(Effect effect, IReadOnlyList<CombatantId> targets, CombatantId? caster)
    {
        Effect = effect;
        Targets = targets;
        Caster = caster;
    }
}

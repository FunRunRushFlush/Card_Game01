using System.Collections.Generic;

public class PerformEffectsGA : GameAction, IHaveCaster
{
    public Effect Effect { get; private set; }
    public List<CombatantView> Targets { get; private set; }

    public CombatantView Caster { get; private set; }

    public PerformEffectsGA(Effect effect, List<CombatantView> targets, CombatantView caster)
    {
        Effect = effect;
        Targets = targets == null ? null : new List<CombatantView>(targets);
        Caster = caster;
    }

    // Convenience overload( but prefer passing caster explicitly!)
    public PerformEffectsGA(Effect effect, List<CombatantView> targets) : this(effect, targets, null) { }
}

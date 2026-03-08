using System.Collections.Generic;

public class FinisherDamageGA : GameAction
{
    public IReadOnlyList<CombatantId> Targets { get; }
    public CombatantId? Caster { get; }
    public int BaseDamage { get; }
    public int DamagePerComboPoint { get; }
    public bool ConsumeAllComboPoints { get; }

    public FinisherDamageGA(
        IReadOnlyList<CombatantId> targets,
        CombatantId? caster,
        int baseDamage,
        int damagePerComboPoint,
        bool consumeAllComboPoints)
    {
        Targets = targets;
        Caster = caster;
        BaseDamage = baseDamage;
        DamagePerComboPoint = damagePerComboPoint;
        ConsumeAllComboPoints = consumeAllComboPoints;
    }
}
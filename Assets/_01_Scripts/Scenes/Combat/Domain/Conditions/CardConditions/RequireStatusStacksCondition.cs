using System;
using UnityEngine;


/// <summary>
/// Requires a combatant (caster or manual target) to have at least N stacks of a status.
/// </summary>
[Serializable]
public class RequireStatusStacksCondition : CardCondition
{
    [SerializeField] private ConditionSubject subject = ConditionSubject.Caster;
    [SerializeField] private StatusEffectType statusEffect;
    [SerializeField] private int minStacks = 1;

    public override bool IsMet(in CardPlayabilityContext context)
    {
        var c = subject == ConditionSubject.Caster ? context.Caster : context.ManualTarget;
        if (c == null) return false;

        var combatState = CombatContextService.Instance != null ? CombatContextService.Instance.State : null;
        if (combatState == null) return false;

        if (!combatState.TryGet(c.Id, out var st)) return false;

        return st.GetStatus(statusEffect) >= minStacks;
    }

    public override CardPlayFailReason GetFailReason(in CardPlayabilityContext context)
    {
        string who = subject == ConditionSubject.Caster ? "caster" : "target";
        return new(CardPlayFailCode.RequiresAttackPlayedThisTurn, $"Requires {who} to have {minStacks} {statusEffect}");
    }
}
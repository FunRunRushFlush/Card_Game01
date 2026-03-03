using UnityEngine;


[System.Serializable]
public class TargetHealthPercentBelowCondition : CardCondition
{
    [SerializeField] private ConditionSubject subject = ConditionSubject.ManualTarget;
    [Range(0f, 1f)]
    [SerializeField] private float maxHealthPercent = 0.5f; // 50%

    public override bool IsMet(in CardPlayabilityContext context)
    {
        CombatantView c = subject == ConditionSubject.Caster
            ? context.Caster
            : context.ManualTarget;

        // During StartPlay you might not have a target selected yet -> allow until commit
        if (c == null)
            return context.Phase == CardPlayPhase.StartPlay;

        var combatState = CombatContextService.Instance != null ? CombatContextService.Instance.State : null;
        if (combatState == null) return false;

        if (!combatState.TryGet(c.Id, out var st)) return false;
        if (st.MaxHealth <= 0) return false;

        float pct = (float)st.Health / st.MaxHealth;
        return pct <= maxHealthPercent;
    }

    public override CardPlayFailReason GetFailReason(in CardPlayabilityContext context)
    {
        int pct = Mathf.RoundToInt(maxHealthPercent * 100f);
        return new(CardPlayFailCode.HealthPercentTooHigh, $"Target HP must be ≤ {pct}%");
    }
}

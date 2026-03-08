using UnityEngine;

public class RequireComboPointsCondition : CardCondition
{
    [SerializeField] private int requiredAmount = 1;

    public override bool IsMet(in CardPlayabilityContext context)
    {
        return ComboPointSystem.Instance != null
            && ComboPointSystem.Instance.HasEnough(requiredAmount);
    }

    public override CardPlayFailReason GetFailReason(in CardPlayabilityContext context)
    {
        return new CardPlayFailReason(
            CardPlayFailCode.RequireComboPoints,
            $"Requires {requiredAmount} Combo Point(s)"
        );
    }

}
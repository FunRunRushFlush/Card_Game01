[System.Serializable]
public class RequireAttackPlayedThisTurnCondition : CardCondition
{
    public override bool IsMet(in CardPlayabilityContext context)
    {
        if (RoundStateSystem.Instance == null) return false;
        return RoundStateSystem.Instance.AttacksPlayedThisTurn >= 1;
    }

    public override CardPlayFailReason GetFailReason(in CardPlayabilityContext context)
        => new(CardPlayFailCode.RequiresAttackPlayedThisTurn, "Play an attack first this turn");
}

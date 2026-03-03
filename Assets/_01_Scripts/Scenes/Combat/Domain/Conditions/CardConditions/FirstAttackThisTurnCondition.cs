[System.Serializable]
public class FirstAttackThisTurnCondition : CardCondition
{
    public override bool IsMet(in CardPlayabilityContext context)
    {
        if (RoundStateSystem.Instance == null)
            return false;

        if (context.Card == null || !context.Card.HasTag(CardTag.Attack))
            return true;

        return RoundStateSystem.Instance.AttacksPlayedThisTurn == 0;
    }

    public override CardPlayFailReason GetFailReason(in CardPlayabilityContext context)
        => new(CardPlayFailCode.MustBeFirstAttackThisTurn, "Only playable as the first attack this turn");
}

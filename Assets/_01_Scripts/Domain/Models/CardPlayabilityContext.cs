public readonly struct CardPlayabilityContext
{
    public readonly Card Card;
    public readonly CombatantId CasterId;
    public readonly CombatantId? ManualTargetId;
    public readonly CardPlayPhase Phase;

    public CardPlayabilityContext(
        Card card,
        CombatantId casterId,
        CombatantId? manualTargetId,
        CardPlayPhase phase)
    {
        Card = card;
        CasterId = casterId;
        ManualTargetId = manualTargetId;
        Phase = phase;
    }
}
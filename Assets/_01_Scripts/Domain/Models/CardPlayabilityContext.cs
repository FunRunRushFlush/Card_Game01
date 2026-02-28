

public readonly struct CardPlayabilityContext
{
    public readonly Card Card;
    public readonly CombatantView Caster;
    public readonly EnemyView ManualTarget;
    public readonly CardPlayPhase Phase;

    public CardPlayabilityContext(Card card, CombatantView caster, EnemyView manualTarget, CardPlayPhase phase)
    {
        Card = card;
        Caster = caster;
        ManualTarget = manualTarget;
        Phase = phase;
    }
}
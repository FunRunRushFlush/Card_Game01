public class PlayCardGA : GameAction, IHaveCaster
{
    public EnemyView ManualTarget { get; private set; }
    public Card Card { get; private set; }

    public CombatantView Caster { get; private set; }

    public PlayCardGA(Card card) : this(card, null, null) { }

    public PlayCardGA(Card card, EnemyView manualTarget) : this(card, manualTarget, null) { }

    public PlayCardGA(Card card, CombatantView caster) : this(card, null, caster) { }

    public PlayCardGA(Card card, EnemyView manualTarget, CombatantView caster)
    {
        Card = card;
        ManualTarget = manualTarget;
        Caster = caster;
    }
}


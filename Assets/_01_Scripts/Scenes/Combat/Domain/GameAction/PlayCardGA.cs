public class PlayCardGA : GameAction
{
    public Card Card { get; }
    public CombatantId CasterId { get; }
    public CombatantId? ManualTargetId { get; } // null wenn Karte kein manuelles Target hat

    public PlayCardGA(Card card, CombatantId casterId, CombatantId? manualTargetId = null)
    {
        Card = card;
        CasterId = casterId;
        ManualTargetId = manualTargetId;
    }
}
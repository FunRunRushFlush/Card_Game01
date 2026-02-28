using System.Collections.Generic;

/// <summary>
/// Discards the entire hand.
/// TargetMode: NoTM
/// </summary>
public class DiscardHandEffect : Effect
{
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Discarding the hand is a card-system operation and doesn't need targets.
        return new DiscardAllCardsGA();
    }
}

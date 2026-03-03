using System.Collections.Generic;

/// <summary>
/// Discards the entire hand.
/// TargetMode: NoTM
/// </summary>
public class DiscardHandEffect : Effect
{
    public override GameAction GetGameAction(IReadOnlyList<CombatantId> targets, CombatantId? caster)
        => new DiscardAllCardsGA();
}
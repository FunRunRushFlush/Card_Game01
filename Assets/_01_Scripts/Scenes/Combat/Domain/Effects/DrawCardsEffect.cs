using System.Collections.Generic;
using UnityEngine;

public class DrawCardsEffect : Effect
{
    [SerializeField] private int amount = 1;

    public override GameAction GetGameAction(IReadOnlyList<CombatantId> targets, CombatantId? caster)
        => new DrawCardsGA(amount);
}
using System.Collections.Generic;
using UnityEngine;

public class FinisherDamageEffect : Effect
{
    [SerializeField] private int baseDamage = 4;
    [SerializeField] private int damagePerComboPoint = 3;
    [SerializeField] private bool consumeAllComboPoints = true;

    public override GameAction GetGameAction(IReadOnlyList<CombatantId> targets, CombatantId? caster)
    {
        int comboPoints = ComboPointSystem.Instance != null
            ? ComboPointSystem.Instance.CurrentComboPoints
            : 0;

        int damage = baseDamage + comboPoints * damagePerComboPoint;

        return new FinisherDamageGA(targets, caster, baseDamage, damagePerComboPoint, consumeAllComboPoints);
    }
}
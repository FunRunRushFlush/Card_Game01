using System;
using System.Collections.Generic;
using UnityEngine;

public class Perk
{
    public Sprite Image => _data.Image;

    private readonly PerkData _data;
    private readonly PerkCondition _perkCondition;
    private readonly AutoTargetEffect _autoTargetEffect;

    private IDisposable subscription;

    public Perk(PerkData perkData)
    {
        _data = perkData;
        _perkCondition = perkData.PerkCondition;
        _autoTargetEffect = perkData.AutoTargetEffect;
    }

    public void OnAdd()
    {
        subscription = _perkCondition.SubscribeCondition(Reaction);
    }

    public void OnRemove()
    {
        subscription?.Dispose();
        subscription = null;
    }

    private void Reaction(GameAction gameAction)
    {
        if (!_perkCondition.SubConditionIsMet(gameAction))
            return;

        var targetIds = new List<CombatantId>();

        // Optional: use action caster as target
        if (_data.UseActionCasterAsTarget && gameAction is IHaveCaster haveCaster)
        {
            if (haveCaster.Caster.HasValue)
                targetIds.Add(haveCaster.Caster.Value);
        }

        // Optional: auto target mode
        if (_data.UseAutoTarget)
        {
            var autoIds = _autoTargetEffect.TargetMode.GetTargetIds();
            if (autoIds != null && autoIds.Count > 0)
                targetIds.AddRange(autoIds);
        }

        // If no targets were collected, do nothing (prevents null/empty issues)
        if (targetIds.Count == 0)
            return;

        // Perk caster is the hero (domain id)
        var heroCasterId = (CombatantId?)HeroSystem.Instance.HeroView.Id;

        GameAction perkEffectAction = _autoTargetEffect.Effect.GetGameAction(targetIds, heroCasterId);
        ActionSystem.Instance.AddReaction(perkEffectAction);
    }
}
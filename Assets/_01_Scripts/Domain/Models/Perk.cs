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
        if (_perkCondition.SubConditionIsMet(gameAction))
        {
            List<CombatantView> targets = new();

            if (_data.UseActionCasterAsTarget && gameAction is IHaveCaster haveCaster)
                targets.Add(haveCaster.Caster);

            if (_data.UseAutoTarget)
                targets.AddRange(_autoTargetEffect.TargetMode.GetTargets());

            GameAction perkEffectAction = _autoTargetEffect.Effect.GetGameAction(targets, HeroSystem.Instance.HeroView);
            ActionSystem.Instance.AddReaction(perkEffectAction);
        }
    }
}

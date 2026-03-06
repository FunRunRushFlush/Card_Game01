using System.Collections;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
    }

    private IEnumerator DealDamagePerformer(DealDamageGA ga)
    {
        var state = CombatContextService.Instance.State;
        if (state == null)
        {
            Debug.LogError("DamageSystem: CombatContextService.State is NULL.");
            yield break;
        }

        if (ga.Targets == null || ga.Targets.Count == 0)
            yield break;

        int baseAmount = ga.Amount;

        int str = 0;
        int weak = 0;

        if (ga.Caster.HasValue && state.TryGet(ga.Caster.Value, out var casterState))
        {
            str = casterState.GetStatus(StatusEffectType.STRENGTH);
            weak = casterState.GetStatus(StatusEffectType.WEAKNESS);
        }

        int modifiedAmount = DamageCalculator.Calculate(baseAmount, str, weak);

        foreach (var targetId in ga.Targets)
        {
            if (!state.TryGet(targetId, out var targetState))
                continue;

            var wasAlive = targetState.Health > 0;
            targetState.TakeDamage(modifiedAmount);


            // notify presentation
            CombatDomainEventBus.Publish(new DamageAppliedEvent(targetId, modifiedAmount, ga.Caster));
            CombatDomainEventBus.Publish(new CombatantStateChangedEvent(targetId));

            if (wasAlive && targetState.Health <= 0)
                ActionSystem.Instance.AddReaction(new ResolveDeathGA(targetId));

            yield return null;
        }
    }
}
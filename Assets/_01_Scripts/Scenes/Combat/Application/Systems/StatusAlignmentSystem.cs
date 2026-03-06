using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusAlignmentSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyBurnGA>(ApplyBurnPerformer);
        ActionSystem.AttachPerformer<ApplyPoisonGA>(ApplyPoisonPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyBurnGA>();
        ActionSystem.DetachPerformer<ApplyPoisonGA>();
    }

    private IEnumerator ApplyBurnPerformer(ApplyBurnGA ga)
    {
        var id = ga.Target;

        if (!CombatContextService.Instance.State.TryGet(id, out var st) || st.Health <= 0)
            yield break;

        CombatDomainEventBus.Publish(
            new StatusTickVisualRequestedEvent(id, StatusEffectType.BURN)
        );

        // Burn tick also consumes 1 stack
        st.RemoveStatus(StatusEffectType.BURN, 1);

        CombatDomainEventBus.Publish(
            new StatusRemovedEvent(id, StatusEffectType.BURN, 1)
        );

        CombatDomainEventBus.Publish(
            new CombatantStateChangedEvent(id)
        );

        ActionSystem.Instance.AddReaction(
            new DealDamageGA(ga.BurnDamage, new List<CombatantId> { id }, caster: null)
        );

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ApplyPoisonPerformer(ApplyPoisonGA ga)
    {
        var id = ga.Target;

        if (!CombatContextService.Instance.State.TryGet(id, out var st) || st.Health <= 0)
            yield break;

        CombatDomainEventBus.Publish(
            new StatusTickVisualRequestedEvent(id, StatusEffectType.POISON)
        );

        ActionSystem.Instance.AddReaction(
            new DealDamageGA(ga.PoisonDamage, new List<CombatantId> { id }, caster: null)
        );

        yield return new WaitForSeconds(1f);
    }
}
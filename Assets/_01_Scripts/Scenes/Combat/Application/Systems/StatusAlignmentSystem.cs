using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusAlignmentSystem : MonoBehaviour
{
    [SerializeField] private GameObject burnVFX;
    [SerializeField] private GameObject poisonVFX;
    [SerializeField] private CombatantViewRegistry viewRegistry;


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

        if (viewRegistry && viewRegistry.TryGet(id, out var view) && view)
        {
            if (burnVFX) Instantiate(burnVFX, view.transform.position, Quaternion.identity);
        }

        // Burn tick also consumes 1 stack (like your old code)
        st.RemoveStatus(StatusEffectType.BURN, 1);

        if (viewRegistry && viewRegistry.TryGet(id, out var view2) && view2)
            view2.Render(st);

        ActionSystem.Instance.AddReaction(new DealDamageGA(ga.BurnDamage, new List<CombatantId> { id }, caster: null));

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ApplyPoisonPerformer(ApplyPoisonGA ga)
    {
        var id = ga.Target;

        if (!CombatContextService.Instance.State.TryGet(id, out var st) || st.Health <= 0)
            yield break;

        if (viewRegistry && viewRegistry.TryGet(id, out var view) && view)
        {
            if (poisonVFX) Instantiate(poisonVFX, view.transform.position, Quaternion.identity);
        }

        ActionSystem.Instance.AddReaction(new DealDamageGA(ga.PoisonDamage, new List<CombatantId> { id }, caster: null));

        yield return new WaitForSeconds(1f);
    }
}
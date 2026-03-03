using System.Collections;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private CombatantViewRegistry viewRegistry;



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
            Debug.LogError("DamageSystem: CombatStateSystem.State is NULL. Did you forget to initialize it in CombatBootstrapper?");
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

            // 1) Domain: Schaden anwenden
            targetState.TakeDamage(modifiedAmount);

            // 2) Presentation: Feedback + VFX + UI rendern
            if (viewRegistry != null && viewRegistry.TryGet(targetId, out var view) && view != null)
            {
                view.PlayHitFeedback();

                if (damageVFX)
                    Instantiate(damageVFX, view.transform.position, Quaternion.identity);

                view.Render(targetState);
            }

            yield return new WaitForSeconds(0.15f);

            // 3) Death-Reaction jetzt idealerweise ebenfalls über ID:
            ActionSystem.Instance.AddReaction(new ResolveDeathGA(targetId));
        }
    }
}
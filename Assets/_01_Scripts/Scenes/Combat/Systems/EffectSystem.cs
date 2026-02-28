using System.Collections;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformEffectsGA>(PerformEffectPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformEffectsGA>();
    }

    private IEnumerator PerformEffectPerformer(PerformEffectsGA performEffectsGA)
    {
        Debug.Log($"[EffectSystem] PerformEffectsGA: effect={performEffectsGA.Effect?.GetType().Name} targets={(performEffectsGA.Targets == null ? 0 : performEffectsGA.Targets.Count)} caster={(performEffectsGA.Caster != null ? performEffectsGA.Caster.name : "null")}");

        // Fallback keeps current behaviour if a caster was not provided.
        CombatantView caster = performEffectsGA.Caster ?? HeroSystem.Instance.HeroView;

        GameAction effectAction = performEffectsGA.Effect.GetGameAction(performEffectsGA.Targets, caster);
        ActionSystem.Instance.AddReaction(effectAction);
        Debug.Log($"[EffectSystem] Produced action: {(effectAction == null ? "NULL" : effectAction.GetType().Name)}");


        yield return null;
    }
}

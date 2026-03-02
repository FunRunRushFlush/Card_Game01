using System.Collections;
using Game.Logging;
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
        Log.Debug(LogArea.Combat, () =>
            $"PerformEffectsGA: effect={performEffectsGA.Effect?.GetType().Name} targets={(performEffectsGA.Targets == null ? 0 : performEffectsGA.Targets.Count)} caster={(performEffectsGA.Caster != null ? performEffectsGA.Caster.name : "null")}",
            this);

        // Fallback keeps current behaviour if a caster was not provided.
        CombatantView caster = performEffectsGA.Caster ?? HeroSystem.Instance.HeroView;

        GameAction effectAction = performEffectsGA.Effect.GetGameAction(performEffectsGA.Targets, caster);
        ActionSystem.Instance.AddReaction(effectAction);

        Log.Debug(LogArea.Combat, () =>
            $"Produced action: {(effectAction == null ? "NULL" : effectAction.GetType().Name)}",
            this);

        yield return null;
    }
}
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

    private IEnumerator PerformEffectPerformer(PerformEffectsGA ga)
    {
        // Fallback keeps current behaviour if a caster was not provided.
        var casterId = ga.Caster ?? CombatantIds.Hero;

        Log.Debug(LogArea.Combat, () =>
            $"PerformEffectsGA: effect={ga.Effect?.GetType().Name} targets={(ga.Targets == null ? 0 : ga.Targets.Count)} casterId={(casterId.Value)}",
            this);

        var effectAction = ga.Effect.GetGameAction(ga.Targets, casterId);
        ActionSystem.Instance.AddReaction(effectAction);

        Log.Debug(LogArea.Combat, () =>
            $"Produced action: {(effectAction == null ? "NULL" : effectAction.GetType().Name)}",
            this);

        yield return null;
    }
}
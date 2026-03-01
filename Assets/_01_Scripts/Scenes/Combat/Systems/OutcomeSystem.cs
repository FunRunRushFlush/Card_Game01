using System.Collections;
using Game.Scenes.Core;
using UnityEngine;

public class OutcomeSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ResolveDeathGA>(ResolveDeathPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ResolveDeathGA>();
    }

    private IEnumerator ResolveDeathPerformer(ResolveDeathGA ga)
    {
        var target = ga.Target;
        if (!target) yield break;

        if (target.CurrentHealth > 0) yield break;

        if (target is EnemyView enemy)
        {
            ActionSystem.Instance.AddReaction(new KillEnemyGA(enemy));
            yield break;
        }

        if (target is HeroView)
        {
            GameFlowController.Current.CombatLost();
            yield break;
        }

        Debug.LogError("[OutcomeSystem] Unknown CombatantView type.");
    }
}
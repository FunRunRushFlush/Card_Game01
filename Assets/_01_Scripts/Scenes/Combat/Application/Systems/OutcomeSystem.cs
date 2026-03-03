using System.Collections;
using Game.Logging;
using Game.Scenes.Core;
using UnityEngine;

public class OutcomeSystem : MonoBehaviour
{
    [SerializeField] private CombatantViewRegistry viewRegistry;

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
        var targetId = ga.Target;

        var combatState = CombatContextService.Instance != null ? CombatContextService.Instance.State : null;
        if (combatState == null)
        {
            Log.Error(LogArea.Combat, () => "CombatStateSystem.State is null", this);
            yield break;
        }

        if (!combatState.TryGet(targetId, out var st))
            yield break;

        if (st.Health > 0)
            yield break;

        // Hero defeated?
        var hero = HeroSystem.Instance != null ? HeroSystem.Instance.HeroView : null;
        if (hero != null && hero.Id.Value == targetId.Value)
        {
            GameFlowController.Current.CombatLost();
            yield break;
        }

        // Enemy defeated
        ActionSystem.Instance.AddReaction(new KillEnemyGA(targetId));
    }
}
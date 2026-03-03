using System.Collections;
using UnityEngine;

public class StatusEffectSystem : MonoBehaviour
{
    [SerializeField] private CombatantViewRegistry viewRegistry;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddStatusEffectGA>(AddStatusEffectPerformer);
        ActionSystem.AttachPerformer<RemoveStatusEffectGA>(RemoveStatusEffectPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddStatusEffectGA>();
        ActionSystem.DetachPerformer<RemoveStatusEffectGA>();
    }

    private IEnumerator AddStatusEffectPerformer(AddStatusEffectGA ga)
    {
        foreach (var targetId in ga.Targets)
        {
            if (!CombatContextService.Instance.State.TryGet(targetId, out var st))
                continue;

            if (st.Health <= 0)
                continue;

            st.AddStatus(ga.StatusEffectType, ga.StackCount);

            if (viewRegistry && viewRegistry.TryGet(targetId, out var view) && view)
                view.Render(st);

            yield return null;
        }
    }

    private IEnumerator RemoveStatusEffectPerformer(RemoveStatusEffectGA ga)
    {
        foreach (var targetId in ga.Targets)
        {
            if (!CombatContextService.Instance.State.TryGet(targetId, out var st))
                continue;

            if (st.Health <= 0)
                continue;

            st.RemoveStatus(ga.StatusEffectType, ga.StackCount);

            if (viewRegistry && viewRegistry.TryGet(targetId, out var view) && view)
                view.Render(st);

            yield return null;
        }
    }
}
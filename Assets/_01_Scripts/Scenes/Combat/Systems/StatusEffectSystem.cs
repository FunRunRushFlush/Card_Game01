using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class StatusEffectSystem : MonoBehaviour
{
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

    private IEnumerator AddStatusEffectPerformer(AddStatusEffectGA addStatusEffectGA)
    {
        foreach (var target in addStatusEffectGA.Targets)
        {
            if (!target) 
                yield break;
            if (target.CurrentHealth <= 0) 
                yield break;

            target.AddStatusEffect(addStatusEffectGA.StatusEffectType, addStatusEffectGA.StackCount);
            //TODO: Animation?
            yield return null;
        }
    }

    private IEnumerator RemoveStatusEffectPerformer(RemoveStatusEffectGA removeStatusEffectGA)
    {
        foreach (var target in removeStatusEffectGA.Targets)
        {
            if (!target)
                yield break;
            if (target.CurrentHealth <= 0)
                yield break;

            target.RemoveStatusEffect(removeStatusEffectGA.StatusEffectType, removeStatusEffectGA.StackCount);
            //TODO: Animation?
            yield return null;
        }
    }

}

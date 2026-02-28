using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusAlignmentSystem : MonoBehaviour
{
    [SerializeField] private GameObject burnVFX;
    [SerializeField] private GameObject poisonVFX;

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
        var target = ga.Target;
        if (!target || target.CurrentHealth <= 0) yield break;

        var guardCheck = target.transform;        
        if (guardCheck) 
        { 
            Instantiate(burnVFX, guardCheck.position, Quaternion.identity);
        }


        if (!target) 
            yield break;

        target.RemoveStatusEffect(StatusEffectType.BURN, 1);


        if (!target) 
            yield break;

        ActionSystem.Instance.AddReaction(
            new DealDamageGA(ga.BurnDamage, new() { target }, caster: null)
        );

        yield return new WaitForSeconds(1f);
    }
    private IEnumerator ApplyPoisonPerformer(ApplyPoisonGA ga)
    {
        var target = ga.Target;
        if (!target || target.CurrentHealth <= 0) yield break;

        var guardCheck = target.transform;
        if (guardCheck) 
            Instantiate(poisonVFX, guardCheck.position, Quaternion.identity);

        if (!target) yield break;
        ActionSystem.Instance.AddReaction(
            new DealDamageGA(ga.PoisonDamage, new() { target }, caster: null)
        );

        yield return new WaitForSeconds(1f);
    }
}

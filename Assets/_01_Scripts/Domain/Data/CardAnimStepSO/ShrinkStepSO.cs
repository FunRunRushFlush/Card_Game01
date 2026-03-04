using System.Collections;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Card Animation Steps/Shrink")]
public class ShrinkStepSO : CardAnimStepSO
{
    [SerializeField] private Vector3 targetScale = Vector3.zero;
    [SerializeField] private float duration = 0.18f;

    public override IEnumerator Play(CardAnimContext ctx)
    {
        if (ctx?.CardTransform == null) yield break;

        ctx.CardTransform.DOKill();

        var t = ctx.CardTransform.DOScale(targetScale, duration);
        yield return t.WaitForCompletion();
    }
}
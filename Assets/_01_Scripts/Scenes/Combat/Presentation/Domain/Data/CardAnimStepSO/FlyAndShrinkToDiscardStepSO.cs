using System.Collections;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Card Animation Steps/Fly+Shrink To Discard")]
public class FlyAndShrinkToDiscardStepSO : CardAnimStepSO
{
    [SerializeField] private float duration = 0.18f;

    public override IEnumerator Play(CardAnimContext ctx)
    {
        if (ctx?.CardTransform == null) yield break;

        ctx.CardTransform.DOKill();

        var move = ctx.CardTransform.DOMove(ctx.DiscardPosition, duration);
        ctx.CardTransform.DOScale(Vector3.zero, duration);

        yield return move.WaitForCompletion();
    }
}
using System.Collections;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Card Animation Steps/Fly To Discard")]
public class FlyToDiscardStepSO : CardAnimStepSO
{
    [SerializeField] private float duration = 0.18f;

    public override IEnumerator Play(CardAnimContext ctx)
    {
        if (ctx?.CardTransform == null) yield break;

        ctx.CardTransform.DOKill();

        var t = ctx.CardTransform.DOMove(ctx.DiscardPosition, duration);
        yield return t.WaitForCompletion();
    }
}
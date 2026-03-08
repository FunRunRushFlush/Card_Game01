using System.Collections;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Card Animation Steps/Fly To Target")]
public class FlyToTargetStepSO : CardAnimStepSO
{
    [SerializeField] private float duration = 0.18f;

    public override IEnumerator Play(CardAnimContext ctx)
    {
        if (ctx?.CardTransform == null || ctx?.Presentation == null) yield break;
        if (!ctx.TargetId.HasValue) yield break;

        if (!ctx.Presentation.TryGetCombatantWorldPosition(ctx.TargetId.Value, out var pos))
            yield break;

        ctx.CardTransform.DOKill();

        var t = ctx.CardTransform.DOMove(pos, duration);
        yield return t.WaitForCompletion();
    }
}
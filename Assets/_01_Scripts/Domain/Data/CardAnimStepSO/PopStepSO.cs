using System.Collections;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Card Animation Steps/Pop")]
public class PopStepSO : CardAnimStepSO
{
    [SerializeField] private float scale = 1.05f;
    [SerializeField] private float duration = 0.08f;

    public override IEnumerator Play(CardAnimContext ctx)
    {
        if (ctx?.CardTransform == null) yield break;

        ctx.CardTransform.DOKill();

        var t = ctx.CardTransform.DOScale(scale, duration);
        yield return t.WaitForCompletion();
    }
}
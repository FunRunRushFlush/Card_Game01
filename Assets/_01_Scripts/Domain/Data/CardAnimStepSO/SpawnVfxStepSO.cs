using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Card Animation Steps/Spawn VFX (at card)")]
public class SpawnVfxStepSO : CardAnimStepSO
{
    [SerializeField] private GameObject prefab;

    public override IEnumerator Play(CardAnimContext ctx)
    {
        if (prefab == null || ctx?.CardTransform == null) yield break;

        Object.Instantiate(prefab, ctx.CardTransform.position, Quaternion.identity);
        yield break;
    }
}
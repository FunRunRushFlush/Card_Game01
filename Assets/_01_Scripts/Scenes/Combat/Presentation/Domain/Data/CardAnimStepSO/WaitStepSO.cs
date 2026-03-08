using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Card Animation Steps/Wait")]
public class WaitStepSO : CardAnimStepSO
{
    [SerializeField] private float seconds = 0.05f;

    public override IEnumerator Play(CardAnimContext ctx)
    {
        if (seconds > 0f)
            yield return new WaitForSeconds(seconds);
    }
}
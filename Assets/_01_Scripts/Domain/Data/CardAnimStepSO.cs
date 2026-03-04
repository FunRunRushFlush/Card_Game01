using System.Collections;
using UnityEngine;

public abstract class CardAnimStepSO : ScriptableObject
{
    public abstract IEnumerator Play(CardAnimContext ctx);
}
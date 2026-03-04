using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Card Animation Sequence")]
public class CardAnimationSequenceSO : ScriptableObject
{
    public List<CardAnimStepSO> Steps = new();
}
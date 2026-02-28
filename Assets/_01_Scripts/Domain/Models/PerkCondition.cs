using System;
using UnityEngine;

[System.Serializable]
public abstract class PerkCondition
{
    [SerializeField] protected ReactionTiming reactionTiming;
    public abstract IDisposable SubscribeCondition(Action<GameAction> reaction);
    public abstract bool SubConditionIsMet(GameAction gameAction);

}

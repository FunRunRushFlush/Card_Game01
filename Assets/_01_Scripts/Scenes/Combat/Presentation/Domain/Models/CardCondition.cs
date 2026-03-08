using System;
using static UnityEngine.Audio.IAudioGenerator;

/// <summary>
/// Base class for per-card play conditions.
/// Add instances to CardData via SerializeReference.
/// </summary>
[Serializable]
public abstract class CardCondition
{
    /// <summary>
    /// Return true if the condition is met for the given play context.
    /// </summary>
    public abstract bool IsMet(in CardPlayabilityContext context);

    /// <summary>
    /// Human-friendly reason (optional) used for debugging/UI.
    /// </summary>
    public abstract CardPlayFailReason GetFailReason(in CardPlayabilityContext context);
    //public virtual string GetFailReason(in CardPlayabilityContext context)
    //{
    //    return GetType().Name;
    //}
}
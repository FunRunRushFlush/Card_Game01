using UnityEngine;

public sealed class CardAnimContext
{
    public CardView CardView { get; }
    public Vector3 DiscardPosition { get; }
    public CombatantId? TargetId { get; } // optional
    public CombatPresentationController Presentation { get; }

    public Transform CardTransform => CardView != null ? CardView.transform : null;

    public CardAnimContext(
        CombatPresentationController presentation,
        CardView cardView,
        Vector3 discardPosition,
        CombatantId? targetId)
    {
        Presentation = presentation;
        CardView = cardView;
        DiscardPosition = discardPosition;
        TargetId = targetId;
    }
}
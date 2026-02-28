using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;

    [Header("Layout")]
    [SerializeField] private float cardSpacingFactor = 1.0f;
    [SerializeField] private float cardSpacingSmallHandFactor = 1.0f;

    [SerializeField] private float firstCardPositionFactor = 1.0f;
    [SerializeField] private float depthOffset = 0.001f;

    [Header("Animation")]
    [SerializeField] private float snapDuration = 0.15f;
    [SerializeField] private float reorderDuration = 0.10f;
    [SerializeField] private float reorderDeadZone = 0.02f;

    private readonly List<CardView> cards = new();
    private CardView dragging;
    private Sequence layoutSeq;

    // Optional: wenn du mal Slot-Zentren fürs Reordering brauchst
    private readonly List<Vector3> slotPositions = new();

    private void Register(CardView cardView)
    {
        if (cards.Contains(cardView)) return;

        cards.Add(cardView);
        cardView.SetOwnerHand(this);

        RebuildLayout(snapDuration, ignore: null);
    }

    private void Unregister(CardView cardView)
    {
        if (!cards.Remove(cardView)) return;

        if (dragging == cardView)
            dragging = null;

        RebuildLayout(snapDuration, ignore: null);
    }

    public IEnumerator AddCard(CardView cardView)
    {
        Register(cardView);
        yield break;
    }

    public CardView RemoveCard(Card card)
    {
        CardView cv = FindByCard(card);
        if (cv == null) return null;

        Unregister(cv);
        return cv;
    }
    public void BeginDrag(CardView cardView)
    {
        if (!cards.Contains(cardView)) return;
        dragging = cardView;

        // beim Start einmal Layout rechnen, damit SlotPose garantiert frisch ist
        RebuildLayout(0f, ignore: dragging);
    }

    public void Drag(CardView cardView)
    {
        if (dragging != cardView) return;

        TryReorderByX(cardView.transform.position.x);
    }

    public void EndDrag(CardView cardView)
    {
        if (dragging != cardView) return;

        dragging = null;
        RebuildLayout(snapDuration, ignore: null);
    }

    public void CancelDrag(CardView cardView)
    {
        if (dragging == cardView)
            dragging = null;

        RebuildLayout(snapDuration, ignore: null);
    }

    private void TryReorderByX(float draggedX)
    {
        int currentIndex = cards.IndexOf(dragging);
        if (currentIndex < 0) return;

        // Wichtig: Reorder anhand von Slot-Zentren (stabiler als anhand der aktuellen Tween-Positionen)
        if (slotPositions.Count != cards.Count)
            RebuildLayout(0f, ignore: dragging);

        int targetIndex = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == dragging) continue;

            float slotX = slotPositions[i].x;
            if (draggedX > slotX + reorderDeadZone)
                targetIndex++;
        }

        targetIndex = Mathf.Clamp(targetIndex, 0, cards.Count - 1);
        if (targetIndex == currentIndex) return;

        cards.RemoveAt(currentIndex);
        cards.Insert(targetIndex, dragging);

        RebuildLayout(reorderDuration, ignore: dragging);
    }

    private void RebuildLayout(float duration, CardView ignore)
    {
        layoutSeq?.Kill();
        layoutSeq = DOTween.Sequence();

        slotPositions.Clear();
        if (cards.Count == 0) return;


        float cardSpacing = (1f / cards.Count) * cardSpacingFactor;
        float firstT = (0.5f - (cardSpacing * (cards.Count - 1)) / 2f) * firstCardPositionFactor;
        if (cards.Count <= 3)
        {
            cardSpacing = (1f / cards.Count) * cardSpacingFactor * cardSpacingSmallHandFactor;
            firstT = (0.5f - (cardSpacing * (cards.Count - 1)) / 2f) * firstCardPositionFactor;

        }


        Spline spline = splineContainer.Spline;

        for (int i = 0; i < cards.Count; i++)
        {
            CardView cv = cards[i];
            if (cv == null) continue;

            float t = firstT + i * cardSpacing;

            Vector3 splinePos = spline.EvaluatePosition(t);
            Vector3 forward = spline.EvaluateTangent(t);
            Vector3 up = spline.EvaluateUpVector(t);

            Quaternion rot = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            Vector3 worldPos = splinePos + transform.position + depthOffset * i * Vector3.back;

            // SlotPose IMMER setzen (auch für ignore!), damit SnapToSlot nie veraltet ist
            cv.SetHandSlotPose(worldPos, rot);
            slotPositions.Add(worldPos);

            if (cv == ignore)
                continue;

            if (duration <= 0f)
            {
                cv.transform.SetPositionAndRotation(worldPos, rot);
            }
            else
            {
                layoutSeq.Join(cv.transform.DOMove(worldPos, duration));
                layoutSeq.Join(cv.transform.DORotate(rot.eulerAngles, duration));
            }
        }
    }

    // Wenn du weiterhin RemoveCard(Card card) brauchst:
    public CardView FindByCard(Card card) => cards.FirstOrDefault(x => x != null && x.Card == card);
}

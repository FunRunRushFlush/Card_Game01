using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] public GameObject HandCardContrainer;

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

    private readonly List<Vector3> slotPositions = new();

    private void Register(CardView cardView)
    {
        CleanupDeadCards();

        if (cardView == null)
            return;

        if (cards.Contains(cardView))
            return;

        cards.Add(cardView);
        cardView.SetOwnerHand(this);

        RebuildLayout(snapDuration, ignore: null);
    }

    private void Unregister(CardView cardView)
    {
        CleanupDeadCards();

        if (cardView == null)
            return;

        if (!cards.Remove(cardView))
            return;

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
        CleanupDeadCards();

        CardView cv = FindByCard(card);
        if (cv == null)
            return null;

        Unregister(cv);
        return cv;
    }

    public void BeginDrag(CardView cardView)
    {
        CleanupDeadCards();

        if (cardView == null)
            return;

        if (!cards.Contains(cardView))
            return;

        dragging = cardView;
        RebuildLayout(0f, ignore: dragging);
    }

    public void Drag(CardView cardView)
    {
        CleanupDeadCards();

        if (cardView == null)
            return;

        if (dragging != cardView)
            return;

        TryReorderByX(cardView.transform.position.x);
    }

    public void EndDrag(CardView cardView)
    {
        CleanupDeadCards();

        if (dragging != cardView)
            return;

        dragging = null;
        RebuildLayout(snapDuration, ignore: null);
    }

    public void CancelDrag(CardView cardView)
    {
        CleanupDeadCards();

        if (dragging == cardView)
            dragging = null;

        RebuildLayout(snapDuration, ignore: null);
    }

    private void TryReorderByX(float draggedX)
    {
        CleanupDeadCards();

        if (dragging == null)
            return;

        int currentIndex = cards.IndexOf(dragging);
        if (currentIndex < 0)
            return;

        // Nur echte, noch vorhandene Karten berücksichtigen.
        var liveCards = new List<CardView>();
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
                liveCards.Add(cards[i]);
        }

        currentIndex = liveCards.IndexOf(dragging);
        if (currentIndex < 0)
            return;

        // Slot-Positionen sicher neu aufbauen, damit sie zu den liveCards passen.
        RebuildLayout(0f, ignore: dragging);

        if (slotPositions.Count != liveCards.Count)
            return;

        int targetIndex = 0;

        for (int i = 0; i < liveCards.Count; i++)
        {
            if (liveCards[i] == dragging)
                continue;

            float slotX = slotPositions[i].x;
            if (draggedX > slotX + reorderDeadZone)
                targetIndex++;
        }

        targetIndex = Mathf.Clamp(targetIndex, 0, liveCards.Count - 1);
        if (targetIndex == currentIndex)
            return;

        cards.Remove(dragging);
        cards.Insert(targetIndex, dragging);

        RebuildLayout(reorderDuration, ignore: dragging);
    }

    private void RebuildLayout(float duration, CardView ignore)
    {
        CleanupDeadCards();

        layoutSeq?.Kill();
        layoutSeq = DOTween.Sequence();

        slotPositions.Clear();
        if (cards.Count == 0)
            return;

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
            if (cv == null)
                continue;

            float t = firstT + i * cardSpacing;

            Vector3 splinePos = spline.EvaluatePosition(t);
            Vector3 forward = spline.EvaluateTangent(t);
            Vector3 up = spline.EvaluateUpVector(t);

            Quaternion rot = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            Vector3 worldPos = splinePos + transform.position + depthOffset * i * Vector3.back;

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

    private void CleanupDeadCards()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (cards[i] == null)
                cards.RemoveAt(i);
        }

        if (dragging == null)
            return;

        if (!cards.Contains(dragging))
            dragging = null;
    }

    public void CancelAllDragging()
    {
        CleanupDeadCards();

        if (dragging != null)
        {
            CancelDrag(dragging);
            dragging = null;
        }
    }

    public CardView FindByCard(Card card)
        => cards.FirstOrDefault(x => x != null && x.Card == card);

    public CardView GetCardViewByCardRuntimeId(long cardRuntimeId)
    {
        if (cardRuntimeId <= 0)
            return null;

        return cards.FirstOrDefault(x =>
            x != null &&
            x.Card != null &&
            x.Card.RuntimeId == cardRuntimeId);
    }
}
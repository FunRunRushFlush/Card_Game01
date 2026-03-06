using Game.Logging;
using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text rarity;
    [SerializeField] private TMP_Text title;
    [SerializeField] private LayerMask dropAreaLayerMask;

    public Card Card { get; private set; }

    private CardGlow glow;
    private HandView ownerHand;

    private enum CardState { Idle, Dragging, Targeting }
    private CardState state = CardState.Idle;

    private bool cancelRequested;

    private Vector3 slotPos;
    private Quaternion slotRot;

    public void SetOwnerHand(HandView hand) => ownerHand = hand;

    public void SetHandSlotPose(Vector3 pos, Quaternion rot)
    {
        slotPos = pos;
        slotRot = rot;
    }

    private void Awake()
    {
        glow = wrapper.GetComponentInChildren<CardGlow>(true);
    }

    public void Setup(Card card)
    {
        Card = card;
        title.text = card.Title;
        rarity.text = card.Rarity.ToString();
        description.text = card.Description;
        mana.text = card.Mana.ToString();
        imageSR.sprite = card.Image;
    }

    private void Update()
    {
        if (glow == null)
            throw new MissingReferenceException();

        var res = CardPlayabilityService.Instance.EvaluateStart(Card, CombatantIds.Hero);
        glow.SetPlayable(res.CanPlay);

        if (!res.CanPlay)
            Log.Info(LogArea.Combat, () => res.TooltipText(), this);

        if ((state == CardState.Dragging || state == CardState.Targeting) && Input.GetMouseButtonDown(1))
        {
            RequestCancel();
        }
    }

    private void OnMouseEnter()
    {
        if (!InteractionService.Instance.PlayerCanHover()) return;

        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, 0, 0);
        CardViewHoverService.Instance.Show(Card, pos);
    }

    private void OnMouseExit()
    {
        if (!InteractionService.Instance.PlayerCanHover()) return;

        CardViewHoverService.Instance.Hide();
        wrapper.SetActive(true);
    }

    private void OnMouseDown()
    {
        if (!InteractionService.Instance.PlayerCanInteract()) return;

        cancelRequested = false;
        state = CardState.Dragging;

        InteractionService.Instance.PlayerIsDragging = true;

        ownerHand?.BeginDrag(this);

        wrapper.SetActive(true);
        CardViewHoverService.Instance.Hide();

        transform.rotation = Quaternion.identity;
        transform.position = MouseUtil.GetWorldMousePositionInWorldSpace(-5f);
    }

    private void OnMouseDrag()
    {
        if (!InteractionService.Instance.PlayerCanInteract()) return;
        if (!InteractionService.Instance.PlayerIsDragging) return;
        if (cancelRequested) return;

        if (state == CardState.Targeting)
            return;

        if (Card.HasManualTargetEffects
            && !IsTargeting()
            && IsOverDropArea()
            && CardPlayabilityService.Instance.EvaluateStart(Card, CombatantIds.Hero).CanPlay)
        {
            state = CardState.Targeting;

            SnapToSlot();

            ManualTargetService.Instance.StartTargeting(transform.position);
            return;
        }

        transform.position = MouseUtil.GetWorldMousePositionInWorldSpace(-5f);
        ownerHand?.Drag(this);
    }

    private void OnMouseUp()
    {
        if (!InteractionService.Instance.PlayerCanInteract()) return;

        if (cancelRequested)
        {
            FinishInteraction(snap: true);
            return;
        }

        bool played = false;

        if (state == CardState.Targeting)
        {
            EnemyView target = ManualTargetService.Instance.EndTargeting(MouseUtil.GetWorldMousePositionInWorldSpace(-5f));
            CombatantId? targetId = target != null ? (CombatantId?)target.Id : null;

            if (CardPlayabilityService.Instance.EvaluateCommit(Card, CombatantIds.Hero, targetId).CanPlay)
            {
                played = true;
                ActionSystem.Instance.Perform(new PlayCardGA(Card, CombatantIds.Hero, targetId));
                ownerHand?.CancelDrag(this);
            }
            else
            {
                FinishInteraction(snap: true);
            }
        }
        else
        {
            if (IsOverDropArea() && CardPlayabilityService.Instance.EvaluateCommit(Card, CombatantIds.Hero, null).CanPlay)
            {
                played = true;
                ActionSystem.Instance.Perform(new PlayCardGA(Card, CombatantIds.Hero));
                ownerHand?.CancelDrag(this);
            }
            else
            {
                FinishInteraction(snap: true);
            }
        }

        if (!played)
            state = CardState.Idle;
    }

    private void RequestCancel()
    {
        cancelRequested = true;

        if (state == CardState.Targeting)
        {
            ManualTargetService.Instance.CancelTargeting();
        }

        FinishInteraction(snap: true);
    }

    private void FinishInteraction(bool snap)
    {
        InteractionService.Instance.PlayerIsDragging = false;

        if (snap)
        {
            ownerHand?.CancelDrag(this);
            SnapToSlot();
        }

        state = CardState.Idle;
        cancelRequested = false;

        CardViewHoverService.Instance.Hide();
        wrapper.SetActive(true);
    }

    private void SnapToSlot()
    {
        transform.SetPositionAndRotation(slotPos, slotRot);
    }

    private bool IsOverDropArea()
    {
        return Physics.Raycast(transform.position, Vector3.forward, out _, 10f, dropAreaLayerMask);
    }

    private bool IsTargeting() => state == CardState.Targeting;
}
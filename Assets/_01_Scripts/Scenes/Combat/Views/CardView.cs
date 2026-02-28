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

        var res = CardPlayabilitySystem.Instance.EvaluateStart(Card, HeroSystem.Instance.HeroView);
        glow.SetPlayable(res.CanPlay);
        if (!res.CanPlay)
            Debug.Log(res.TooltipText());

        // Right click cancels only if we are interacting with THIS card
        if ((state == CardState.Dragging || state == CardState.Targeting) && Input.GetMouseButtonDown(1))
        {
            RequestCancel();
        }
    }

    private void OnMouseEnter()
    {
        if (!InteractionSystem.Instance.PlayerCanHover()) return;

        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, 0, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    private void OnMouseExit()
    {
        if (!InteractionSystem.Instance.PlayerCanHover()) return;

        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    private void OnMouseDown()
    {
        if (!InteractionSystem.Instance.PlayerCanInteract()) return;

        cancelRequested = false;
        state = CardState.Dragging;

        InteractionSystem.Instance.PlayerIsDragging = true;

        ownerHand?.BeginDrag(this);

        wrapper.SetActive(true);
        CardViewHoverSystem.Instance.Hide();

        // begin at mouse
        transform.rotation = Quaternion.identity;
        transform.position = MouseUtil.GetWorldMousePositionInWorldSpace(-5f);
    }

    private void OnMouseDrag()
    {
        if (!InteractionSystem.Instance.PlayerCanInteract()) return;

        if (!InteractionSystem.Instance.PlayerIsDragging) return;
        if (cancelRequested) return;

        if (state == CardState.Targeting)
            return;


        if (Card.HasManualTargetEffects && !IsTargeting() && IsOverDropArea() && CardPlayabilitySystem.Instance.EvaluateStart(Card, HeroSystem.Instance.HeroView).CanPlay)
        {
            state = CardState.Targeting;


            SnapToSlot();

            ManualTargetSystem.Instance.StartTargeting(transform.position);
            return;
        }

        // Normal dragging
        transform.position = MouseUtil.GetWorldMousePositionInWorldSpace(-5f);
        ownerHand?.Drag(this);
    }

    private void OnMouseUp()
    {
        if (!InteractionSystem.Instance.PlayerCanInteract()) return;

        // Cancel must suppress any play logic
        if (cancelRequested)
        {
            FinishInteraction(snap: true);
            return;
        }

        bool played = false;

        if (state == CardState.Targeting)
        {
            EnemyView target = ManualTargetSystem.Instance.EndTargeting(MouseUtil.GetWorldMousePositionInWorldSpace(-5f));
            if (CardPlayabilitySystem.Instance.EvaluateCommit(Card, HeroSystem.Instance.HeroView, target).CanPlay)
            {
                played = true;
                ActionSystem.Instance.Perform(new PlayCardGA(Card, target, HeroSystem.Instance.HeroView));
                ownerHand?.CancelDrag(this); // remove will follow elsewhere
            }
            else
            {
                // not played => just snap back into hand
                FinishInteraction(snap: true);
            }
        }
        else // Dragging
        {
            if (IsOverDropArea() && CardPlayabilitySystem.Instance.EvaluateCommit(Card, HeroSystem.Instance.HeroView, null).CanPlay)
            {
                played = true;
                ActionSystem.Instance.Perform(new PlayCardGA(Card, HeroSystem.Instance.HeroView));
                ownerHand?.CancelDrag(this); // remove will follow elsewhere
            }
            else
            {
                FinishInteraction(snap: true);
            }
        }

        // If played, you typically remove the card from hand via systems.
        if (!played)
            state = CardState.Idle;
    }

    private void RequestCancel()
    {
        cancelRequested = true;


        if (state == CardState.Targeting)
        {
            //ManualTargetSystem.Instance.EndTargeting(MouseUtil.GetWorldMousePositionInWorldSpace(-5f));
            ManualTargetSystem.Instance.CancelTargeting();

        }

        FinishInteraction(snap: true);
    }

    private void FinishInteraction(bool snap)
    {
        InteractionSystem.Instance.PlayerIsDragging = false;

        if (snap)
        {
            ownerHand?.CancelDrag(this);
            SnapToSlot();
        }

        state = CardState.Idle;
        cancelRequested = false;

        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    private void SnapToSlot()
    {
        // slotPos/slotRot werden vom HandView gepflegt
        transform.SetPositionAndRotation(slotPos, slotRot);
    }

    private bool IsOverDropArea()
    {
        return Physics.Raycast(transform.position, Vector3.forward, out _, 10f, dropAreaLayerMask);
    }

    private bool IsTargeting() => state == CardState.Targeting;


}

using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;
    public Vector3 CardPositionOffset = Vector3.zero;

    public void Show(Card card, Vector3 position)
    {
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card);
        cardViewHover.transform.position = position + CardPositionOffset;
    }

    public void Hide()
    {
        cardViewHover.gameObject.SetActive(false);
    }
}

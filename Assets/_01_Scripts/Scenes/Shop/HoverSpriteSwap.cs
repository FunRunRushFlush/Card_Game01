using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HoverSpriteSwap : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite hoverSprite;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (normalSprite == null) 
            normalSprite = sr.sprite; 
    }

    void OnMouseEnter()
    {
        if (hoverSprite != null) 
            sr.sprite = hoverSprite;
    }

    void OnMouseExit()
    {
        if (normalSprite != null) 
            sr.sprite = normalSprite;
    }
}



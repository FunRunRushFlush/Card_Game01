using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardViewUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image artwork;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text rarity;
    [SerializeField] private TMP_Text title;

    [Header("Selection")]
    [SerializeField] private GameObject selectedBorder; // rotes Border-Objekt

    [Header("Anim")]
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float animDuration = 0.12f;
    [SerializeField] private float hoverTilt = 3f; // leichtes "wackeln" (Rotation)

    public event Action<CardViewUI> Clicked;
    public Card Card { get; private set; }
    public bool IsSelected { get; private set; }

    private RectTransform rt;
    private Vector3 baseScale;
    private Quaternion baseRot;
    private Tween hoverTween;
    private Tween tiltTween;

    private void Awake()
    {
        rt = (RectTransform)transform;
        baseScale = rt.localScale;
        baseRot = rt.localRotation;

        if (selectedBorder != null)
            selectedBorder.SetActive(false);
    }

    public void Setup(Card card)
    {
        Card = card;
        title.text = card.Title;
        rarity.text = card.Rarity.ToString();
        description.text = card.Description;
        mana.text = card.Mana.ToString();
        artwork.sprite = card.Image;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (selectedBorder != null)
            selectedBorder.SetActive(selected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverTween?.Kill();
        tiltTween?.Kill();

        hoverTween = rt.DOScale(baseScale * hoverScale, animDuration).SetEase(Ease.OutQuad);

        // kleines "wobble": einmal hin und her
        tiltTween = rt.DOLocalRotateQuaternion(Quaternion.Euler(0, 0, hoverTilt) * baseRot, animDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                rt.DOLocalRotateQuaternion(baseRot, animDuration).SetEase(Ease.OutQuad);
            });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverTween?.Kill();
        tiltTween?.Kill();

        hoverTween = rt.DOScale(baseScale, animDuration).SetEase(Ease.OutQuad);
        tiltTween = rt.DOLocalRotateQuaternion(baseRot, animDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }
}

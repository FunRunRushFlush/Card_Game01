using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color disabledColor = Color.gray;

    private bool _interactable = true;

    private void Reset()
    {
        if (!label) label = GetComponentInChildren<TMP_Text>();
    }

    public void SetInteractable(bool interactable)
    {
        _interactable = interactable;
        label.color = interactable ? normalColor : disabledColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_interactable) label.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_interactable) label.color = normalColor;
    }
}

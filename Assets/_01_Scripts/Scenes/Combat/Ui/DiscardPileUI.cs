using TMPro;
using UnityEngine;

public class DiscardPileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text discardPile;

    private void Start()
    {
        CardSystem.Instance.DiscardPileCountChanged += UpdateDiscardPileText;
    }

    private void OnDisable()
    {
        if (CardSystem.Instance == null) return;
        CardSystem.Instance.DiscardPileCountChanged -= UpdateDiscardPileText;
    }

    public void UpdateDiscardPileText(int count)
    {
        discardPile.text = count.ToString();
    }
}

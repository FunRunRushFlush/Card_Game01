using TMPro;
using UnityEngine;

public class DrawPileUI : MonoBehaviour
{

    [SerializeField] private TMP_Text drawPile;
    private void Start()
    {
        CardSystem.Instance.DrawPileCountChanged += UpdateDrawPileText;
    }

    private void OnDisable()
    {
        if (CardSystem.Instance == null) return;

        CardSystem.Instance.DrawPileCountChanged -= UpdateDrawPileText;
    }

    public void UpdateDrawPileText(int count)
    {
        drawPile.text = count.ToString();
    }
}

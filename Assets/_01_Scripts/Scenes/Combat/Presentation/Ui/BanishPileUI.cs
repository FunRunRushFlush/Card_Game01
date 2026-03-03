using TMPro;
using UnityEngine;

public class BanishPileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text banishPile;

    private void Start()
    {
        CardSystem.Instance.BanishPileCountChanged += UpdateBanishDrawPileText;
    }

    private void OnDisable()
    {
        if (CardSystem.Instance == null) return;
        CardSystem.Instance.BanishPileCountChanged -= UpdateBanishDrawPileText;
    }

    public void UpdateBanishDrawPileText(int count)
    {
        banishPile.text = count.ToString();
    }
}

using TMPro;
using UnityEngine;

public class HandPileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text handPile;

    private void Start()
    {
        CardSystem.Instance.HandCountChanged += UpdateHandPileText;
    }

    private void OnDisable()
    {
        if (CardSystem.Instance == null) return;

        CardSystem.Instance.HandCountChanged -= UpdateHandPileText;
    }

    public void UpdateHandPileText(int count)
    {
        handPile.text = count.ToString();
    }
}

using TMPro;
using UnityEngine;

public class ComboPointsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text comboPointsText;

    public void Render(int currentComboPoints, int maxComboPoints)
    {
        if (comboPointsText != null)
        {
            comboPointsText.text = $"{currentComboPoints}/{maxComboPoints}";
        }
    }
}
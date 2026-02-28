using UnityEngine;


public class RewardPopUpCanvasUI : MonoBehaviour
{
    [SerializeField] private GameObject rewardPopUpCanvas;

    public void ShowRewardPopUpCanvas()
    {
        if (rewardPopUpCanvas != null)
            rewardPopUpCanvas.SetActive(true);
    }

    public void CloseRewardPopUpCanvas()
    {
        if (rewardPopUpCanvas != null)
            rewardPopUpCanvas.SetActive(false);
    }
}
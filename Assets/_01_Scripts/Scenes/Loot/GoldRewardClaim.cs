using Game.Scenes.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GoldRewardClaim : MonoBehaviour
{
    [Header("Gold Amount")]
    [SerializeField] private int minGold = 20;
    [SerializeField] private int maxGold = 35;

    [Header("UI")]
    [SerializeField] private RewardPopUpCanvasUI popupUI;
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text claimButtonLabel;
    [SerializeField] private RewardEntryUI goldRewardEntry;

    private int _amount;
    private bool _claimed;

    private void Start()
    {
        _amount = Random.Range(minGold, maxGold + 1);

        if (claimButtonLabel != null)
            claimButtonLabel.text = $"+{_amount}";

        if (claimButton != null)
            claimButton.interactable = !_claimed;
    }

    public void ClaimGoldReward()
    {
        if (_claimed) return;

        var session = CoreManager.Instance?.Session;
        if (session == null) return;

        _claimed = true;

        session.Run.ChangeAmountOfGold(_amount);

        if (claimButton != null) claimButton.interactable = false;

        popupUI?.CloseRewardPopUpCanvas();
        goldRewardEntry?.MarkClaimedAndRemove();
    }
}

using Game.Scenes.Core;
using UnityEngine;
using UnityEngine.UI;

public class CardRewardClaim : MonoBehaviour
{
    [Header("Selection")]
    [SerializeField] private MultiSelectionGroup selectionGroup;
    [SerializeField] private int pickCount = 1;

    [Header("UI")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private RewardPopUpCanvasUI popupUI;
    [SerializeField] private RewardEntryUI entryUI;

    [Header("Rewards")]
    [SerializeField] private CardRewardsUI cardRewards;

    private bool _claimed;

    private void Awake()
    {
        if (selectionGroup != null)
            selectionGroup.SetRules(pickCount, canDeselect: true);
    }

    private void OnEnable()
    {
        _claimed = false;

        if (selectionGroup != null)
            selectionGroup.SelectionChanged += OnSelectionChanged;

        OnSelectionChanged();
    }

    private void OnDisable()
    {
        if (selectionGroup != null)
            selectionGroup.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (confirmButton == null || selectionGroup == null) return;
        confirmButton.interactable = !_claimed && (selectionGroup.Selected.Count == pickCount);
    }

    public void ClaimCardReward()
    {
        if (_claimed) return;
        if (selectionGroup == null) return;
        if (selectionGroup.Selected.Count != pickCount) return;

        var session = CoreManager.Instance?.Session;
        if (session == null) return;

        _claimed = true;
        if (confirmButton != null) confirmButton.interactable = false;

        foreach (var view in selectionGroup.Selected)
        {
            var data = view?.Card?.Data;
            if (data != null)
                session.Hero.AddPermanent(data);
        }

        cardRewards?.ConsumeRewardsUI();
        popupUI?.CloseRewardPopUpCanvas();
        entryUI?.MarkClaimedAndRemove();
    }
}

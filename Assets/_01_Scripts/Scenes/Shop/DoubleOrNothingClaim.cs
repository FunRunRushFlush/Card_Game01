using Game.Scenes.Core;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.UI;

public class DoubleOrNothingClaim : MonoBehaviour
{
    [Header("Selection")]
    [SerializeField] private MultiSelectionGroup selectionGroup;
    [SerializeField] private int pickCount = 1;

    [Header("UI")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Deck UI")]
    [SerializeField] private DeckViewUI deckViewUI;

    [Header("Cost")]
    [SerializeField] private int doubleOrNothingCost = 60;

    [Header("Optional: Close panel after action")]
    [SerializeField] private GameObject panelToClose;

    [Header("Optional: Debug / Feedback")]
    [SerializeField] private bool logResult = true;


    private bool _claimed;

    private void Awake()
    {
        if (selectionGroup != null)
            selectionGroup.SetRules(pickCount, canDeselect: true);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Cancel);
    }

    private void OnEnable()
    {
        _claimed = false;

        deckViewUI?.BuildFromCurrentDeck();

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
        if (confirmButton == null || selectionGroup == null)
            return;

        var session = CoreManager.Instance?.Session;
        if (session == null)
        {
            confirmButton.interactable = false;
            return;
        }

        int gold = session.Run.Gold;

        bool hasSelection = selectionGroup.Selected.Count == pickCount;
        bool hasGold = gold >= doubleOrNothingCost;

        confirmButton.interactable = !_claimed && hasSelection && hasGold;
    }

    public void ClaimDoubleOrNothing()
    {
        if (_claimed) return;
        if (selectionGroup == null) return;
        if (selectionGroup.Selected.Count != pickCount) return;

        var session = CoreManager.Instance?.Session;
        if (session == null || session.Hero == null) return;

        int gold = session.Run.Gold;
        if (gold < doubleOrNothingCost) return;

        var selectedView = selectionGroup.Selected[0];
        var data = selectedView?.Card?.Data;
        if (data == null) return;

        _claimed = true;
        if (confirmButton != null) confirmButton.interactable = false;

        // Pay first
        session.Run.ChangeAmountOfGold(-doubleOrNothingCost);

        bool remove = Random.value < 0.5f;

        if (remove)
        {
            session.Hero.RemovePermanent(data);
            if (logResult) Debug.Log($"[DoubleOrNothing] Removed: {data.name}");
        }
        else
        {
            session.Hero.AddPermanent(data);
            if (logResult) Debug.Log($"[DoubleOrNothing] Copied: {data.name}");
        }

        // Refresh deck UI
        deckViewUI?.BuildFromCurrentDeck();

        ClaimEvents.RaiseAnyClaimed();

        if (panelToClose != null)
            panelToClose.SetActive(false);
    }

    private void Cancel()
    {
        if (panelToClose != null)
            panelToClose.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}

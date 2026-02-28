using Game.Scenes.Core;
using UnityEngine;
using UnityEngine.UI;

public class CopyCardClaim : MonoBehaviour
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
    [SerializeField] private int copyCost = 75;

    [Header("Optional: Close panel after copy")]
    [SerializeField] private GameObject panelToClose;

    public event System.Action Claimed;

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
        bool hasGold = gold >= copyCost;

        confirmButton.interactable = !_claimed && hasSelection && hasGold;
    }

    public void ClaimCopyCard()
    {
        if (_claimed) return;
        if (selectionGroup == null) return;
        if (selectionGroup.Selected.Count != pickCount) return;

        var session = CoreManager.Instance?.Session;
        if (session == null || session.Hero == null) return;

        int gold = session.Run.Gold;
        if (gold < copyCost) return;

        _claimed = true;
        if (confirmButton != null) confirmButton.interactable = false;

        session.Run.ChangeAmountOfGold(-copyCost);

        foreach (var view in selectionGroup.Selected)
        {
            var data = view?.Card?.Data;
            if (data != null)
                session.Hero.AddPermanent(data);
        }


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

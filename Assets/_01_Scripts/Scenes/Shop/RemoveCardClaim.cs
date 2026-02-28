using Game.Scenes.Core;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.UI;

public class RemoveCardClaim : MonoBehaviour
{
    [Header("Selection")]
    [SerializeField] private MultiSelectionGroup selectionGroup;
    [SerializeField] private int pickCount = 1;

    [Header("UI")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Deck UI")]
    [SerializeField] private DeckViewUI deckViewUI; // baut das Deck in die Scroll/Grid Ansicht

    [Header("Cost")]
    [SerializeField] private int removeCost = 50;

    [Header("Optional: Close panel after remove")]
    [SerializeField] private GameObject panelToClose; // z.B. dein RemoveCardPanel (optional)

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

        // Deck anzeigen
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
        bool hasGold = gold >= removeCost;

        confirmButton.interactable = !_claimed && hasSelection && hasGold;
    }

    public void ClaimRemoveCard()
    {
        if (_claimed) return;
        if (selectionGroup == null) return;
        if (selectionGroup.Selected.Count != pickCount) return;

        var session = CoreManager.Instance?.Session;
        if (session == null || session.Hero == null) return;

        int gold = session.Run.Gold;
        if (gold < removeCost) return;

        _claimed = true;
        if (confirmButton != null) confirmButton.interactable = false;

        // Gold abziehen
        session.Run.ChangeAmountOfGold(-removeCost);

        // genau 1 Karte entfernen
        foreach (var view in selectionGroup.Selected)
        {
            var data = view?.Card?.Data;
            if (data != null)
                session.Hero.RemovePermanent(data);
        }

        // UI refresh
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

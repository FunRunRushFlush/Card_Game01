using Game.Logging;
using Game.Scenes.Core;
using System.Collections.Generic;
using UnityEngine;

public class DeckViewUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform deckContainer;
    [SerializeField] private MultiSelectionGroup selectionGroup;

    /// <summary>
    /// Builds the UI from the current hero deck.
    /// </summary>
    public void BuildFromCurrentDeck()
    {
        var session = CoreManager.Instance?.Session;
        if (session == null || session.Hero == null)
        {
            Log.Error(LogCat.UI, () => "Session or Hero is null. Cannot build deck UI.", this);
            return;
        }

        BuildFromDeck(session.Hero.Deck);
    }

    /// <summary>
    /// Builds the UI from a provided deck list.
    /// </summary>
    public void BuildFromDeck(IReadOnlyList<CardData> deck)
    {
        if (CardViewCreator.Instance == null)
        {
            Log.Error(LogCat.UI, () => "CardViewCreator.Instance is null.", this);
            return;
        }
        if (deckContainer == null)
        {
            Log.Error(LogCat.UI, () => "deckContainer is null. Assign the ScrollView Content transform.", this);
            return;
        }
        if (selectionGroup == null)
        {
            Log.Error(LogCat.UI, () => "selectionGroup is null. Assign MultiSelectionGroup.", this);
            return;
        }

        ConsumeDeckUI();

        if (deck == null || deck.Count == 0)
        {
            Log.Warn(LogCat.UI, () => "Deck is empty. No cards to display.", this);
            return;
        }

        for (int i = 0; i < deck.Count; i++)
        {
            var data = deck[i];
            if (data == null) continue;

            var card = new Card(data);
            var view = CardViewCreator.Instance.CreateCardViewUI(card, deckContainer);
            if (view != null)
                selectionGroup.Register(view);
        }
    }

    public void ConsumeDeckUI()
    {
        selectionGroup?.Clear();

        if (deckContainer == null) return;

        for (int i = deckContainer.childCount - 1; i >= 0; i--)
            Destroy(deckContainer.GetChild(i).gameObject);
    }
}

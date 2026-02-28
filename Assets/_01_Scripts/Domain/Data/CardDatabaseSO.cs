using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Database")]
public class CardDatabaseSO : ScriptableObject
{
    [SerializeField] private List<CardData> allCards = new();

    [NonSerialized] private Dictionary<string, CardData> byId;

    public IReadOnlyList<CardData> AllCards => allCards;


    private void OnEnable()
    {
        BuildIndex();
    }

    private void BuildIndex()
    {
        byId = new Dictionary<string, CardData>(StringComparer.Ordinal);

        foreach (var c in allCards)
        {
            if (c == null) continue;

            if (string.IsNullOrWhiteSpace(c.Id))
            {
                Debug.LogError($"[CardDatabaseSO] CardData '{c.name}' has empty Id.", c);
                continue;
            }

            if (!byId.TryAdd(c.Id, c))
                Debug.LogWarning($"[CardDatabaseSO] Duplicate Card Id '{c.Id}' on '{c.name}'.", c);
        }

        // Debug.Log($"[CardDatabaseSO] Index built. Cards={byId.Count}");
    }

    public CardData GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        if (byId == null || byId.Count == 0) BuildIndex();
        return byId.TryGetValue(id, out var card) ? card : null;
    }


#if UNITY_EDITOR
    // Helper for editor tools / debug
    public void Editor_SetCards(List<CardData> newCards)
    {
        allCards = newCards;
        BuildIndex();
    }
#endif
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CardDatabase : MonoBehaviour
{
    [SerializeField] private List<CardData> allCards = new();
    public IReadOnlyList<CardData> AllCards => allCards;

#if UNITY_EDITOR
    [Header("DEBUG (Auto-generated)")]
    [SerializeField, ReadOnly] private List<CardData> commonCards = new();
    [SerializeField, ReadOnly] private List<CardData> uncommonCards = new();
    [SerializeField, ReadOnly] private List<CardData> rareCards = new();
    [SerializeField, ReadOnly] private List<CardData> epicCards = new();
    [SerializeField, ReadOnly] private List<CardData> adminCards = new();
    [SerializeField, ReadOnly] private List<CardData> starterCards = new();
#endif

    private Dictionary<string, CardData> byId;

    private void Awake()
    {
        BuildIndex();

#if UNITY_EDITOR
        // Helps if someone edited allCards manually without running Rebuild
        RebuildDebugLists();
#endif
    }

    private void BuildIndex()
    {
        byId = new Dictionary<string, CardData>(allCards.Count);

        foreach (var card in allCards)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.Id))
            {
                Debug.LogError("CardDatabase: card missing or has empty id.");
                continue;
            }

            if (byId.ContainsKey(card.Id))
            {
                Debug.LogError($"CardDatabase: duplicate id '{card.Id}'");
                continue;
            }

            byId.Add(card.Id, card);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Card List (Find all CardData assets)")]
    private void RebuildCardList()
    {
        var guids = AssetDatabase.FindAssets("t:CardData");
        var list = new List<CardData>(guids.Length);

        int missingIdCount = 0;
        int nullAssetCount = 0;
        int duplicateIdCount = 0;

        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<CardData>(path);

            if (asset == null)
            {
                nullAssetCount++;
                Debug.LogError($"CardDatabase: Found null CardData at '{path}'.", this);
                continue;
            }

            // Check: ID vorhanden?
            if (string.IsNullOrWhiteSpace(asset.Id))
            {
                missingIdCount++;
                Debug.LogError(
                    $"CardDatabase: CardData has missing/empty Id at '{path}' (name: '{asset.name}').",
                    asset);
                // Optional: if you prefer to EXCLUDE those cards from the pool, uncomment:
                // continue;
            }
            else
            {
                // Check: Duplicate IDs
                if (!seenIds.Add(asset.Id))
                {
                    duplicateIdCount++;
                    Debug.LogError(
                        $"CardDatabase: Duplicate card Id '{asset.Id}' at '{path}' (name: '{asset.name}').",
                        asset);
                }
            }

            list.Add(asset);
        }

        // Stabil sortieren; leere IDs nach hinten schieben, damit’s nicht crasht
        list.Sort((a, b) =>
        {
            var aId = a != null ? a.Id : null;
            var bId = b != null ? b.Id : null;

            var aEmpty = string.IsNullOrWhiteSpace(aId);
            var bEmpty = string.IsNullOrWhiteSpace(bId);

            if (aEmpty && bEmpty) return 0;
            if (aEmpty) return 1;
            if (bEmpty) return -1;

            return string.Compare(aId, bId, StringComparison.Ordinal);
        });

        allCards = list;

        // Build debug rarity buckets
        RebuildDebugLists();

        // Mark dirty so Unity saves the updated serialized lists
        EditorUtility.SetDirty(this);

        Debug.Log(
            $"CardDatabase: Rebuilt list with {allCards.Count} CardData assets. " +
            $"Issues: missingId={missingIdCount}, duplicateId={duplicateIdCount}, nullAsset={nullAssetCount}. " +
            $"Rarities: Common={commonCards.Count}, Uncommon={uncommonCards.Count}, Rare={rareCards.Count}, " +
            $"Epic={epicCards.Count}, Admin={adminCards.Count}, Starter={starterCards.Count}",
            this);
    }

    private void RebuildDebugLists()
    {
        commonCards.Clear();
        uncommonCards.Clear();
        rareCards.Clear();
        epicCards.Clear();
        adminCards.Clear();
        starterCards.Clear();

        foreach (var card in allCards)
        {
            if (card == null) continue;

            var rarity = card.Rarity; // adjust if your field name differs

            switch (rarity)
            {
                case CardRarity.Common: commonCards.Add(card); break;
                case CardRarity.Uncommon: uncommonCards.Add(card); break;
                case CardRarity.Rare: rareCards.Add(card); break;
                case CardRarity.Epic: epicCards.Add(card); break;
                case CardRarity.Admin: adminCards.Add(card); break;
                case CardRarity.Starter: starterCards.Add(card); break;
                default:
                    Debug.LogWarning($"CardDatabase: Unknown rarity '{rarity}' on '{card.name}'.", card);
                    break;
            }
        }

        static int CompareById(CardData a, CardData b)
        {
            var aId = a != null ? a.Id : null;
            var bId = b != null ? b.Id : null;

            var aEmpty = string.IsNullOrWhiteSpace(aId);
            var bEmpty = string.IsNullOrWhiteSpace(bId);

            if (aEmpty && bEmpty) return 0;
            if (aEmpty) return 1;
            if (bEmpty) return -1;

            return string.Compare(aId, bId, StringComparison.Ordinal);
        }

        commonCards.Sort(CompareById);
        uncommonCards.Sort(CompareById);
        rareCards.Sort(CompareById);
        epicCards.Sort(CompareById);
        adminCards.Sort(CompareById);
        starterCards.Sort(CompareById);
    }
#endif
}

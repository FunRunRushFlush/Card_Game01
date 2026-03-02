using Game.Logging;
using Game.Scenes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatBootstrapper : MonoBehaviour
{
    [Header("Content Lookup")]
    [SerializeField] private CardDatabaseSO cardDatabase;
    [SerializeField] private CombatContentIndex contentIndex;

    [Header("Optional Scene Defaults")]
    [SerializeField] private PerkData perkData;

    private Dictionary<string, EnemyData> enemyById;
    private Dictionary<string, EncounterDefinition> encounterById;
    private Dictionary<int, HeroData> heroById;

    private void Awake()
    {
        BuildIndex();
    }

    private void BuildIndex()
    {
        if (cardDatabase == null)
            Log.Error(LogArea.Combat, () => "[CombatBootstrapper] CardDatabase reference is missing.");

        if (contentIndex == null)
        {
            Log.Error(LogArea.Combat, () => "[CombatBootstrapper] CombatContentIndex reference is missing.");
            enemyById = new Dictionary<string, EnemyData>(StringComparer.Ordinal);
            encounterById = new Dictionary<string, EncounterDefinition>(StringComparer.Ordinal);
            return;
        }

        enemyById = new Dictionary<string, EnemyData>(StringComparer.Ordinal);
        foreach (var e in contentIndex.enemies)
        {
            if (e == null) continue;

            if (string.IsNullOrWhiteSpace(e.Id))
            {
                Log.Error(LogArea.Combat, () => $"[CombatBootstrapper] EnemyData '{e.name}' has empty Id.");
                continue;
            }

            if (!enemyById.ContainsKey(e.Id))
                enemyById.Add(e.Id, e);
            else
                Log.Warn(LogArea.Combat, () => $"[CombatBootstrapper] Duplicate EnemyData Id '{e.Id}' on '{e.name}'.");
        }

        encounterById = new Dictionary<string, EncounterDefinition>(StringComparer.Ordinal);
        foreach (var enc in contentIndex.encounters)
        {
            if (enc == null) continue;

            if (string.IsNullOrWhiteSpace(enc.Id))
            {
                Log.Error(LogArea.Combat, () => $"[CombatBootstrapper] EncounterDefinition '{enc.name}' has empty Id.");
                continue;
            }

            if (!encounterById.ContainsKey(enc.Id))
                encounterById.Add(enc.Id, enc);
            else
                Log.Warn(LogArea.Combat, () => $"[CombatBootstrapper] Duplicate EncounterDefinition Id '{enc.Id}' on '{enc.name}'.");
        }

        heroById = new Dictionary<int, HeroData>();
        foreach (var h in contentIndex.heroes)
        {
            if (h == null) continue;

            var key = (int)h.HeroID;
            if (!heroById.TryAdd(key, h))
                Log.Warn(LogArea.Combat, () => $"[CombatBootstrapper] Duplicate HeroID '{h.HeroID}' on '{h.name}'.");
        }

        Log.Info(LogArea.Combat, () => $"[CombatBootstrapper] Index built. Enemies={enemyById.Count}, Encounters={encounterById.Count}");
    }

    public void StartCombat(CombatSetupSnapshot snapshot)
    {
        if (snapshot == null)
        {
            Log.Error(LogArea.Combat, () => "[CombatBootstrapper] Snapshot is null.");
            return;
        }

        if (contentIndex == null)
        {
            Log.Error(LogArea.Combat, () => "[CombatBootstrapper] CombatContentIndex is not assigned.");
            return;
        }

        if (cardDatabase == null)
        {
            Log.Error(LogArea.Combat, () => "[CombatBootstrapper] CardDatabase is not assigned.");
            return;
        }

        if (!heroById.TryGetValue(snapshot.heroId, out var heroData) || heroData == null)
        {
            Log.Error(LogArea.Combat, () => $"[CombatBootstrapper] Missing HeroData for heroId={snapshot.heroId}. " +
                           "Fix: Rebuild CombatContentIndex and ensure heroes list includes your HeroData assets.");
            return;
        }

        HeroSystem.Instance.Setup(heroData);

        // Resolve Enemies
        var enemies = new List<EnemyData>(snapshot.enemyIds.Count);
        var missingEnemyIds = new List<string>();

        foreach (var id in snapshot.enemyIds)
        {
            if (!enemyById.TryGetValue(id, out var data) || data == null)
                missingEnemyIds.Add(id);
            else
                enemies.Add(data);
        }

        if (missingEnemyIds.Count > 0)
        {
            Log.Error(LogArea.Combat, () => 
                "[CombatBootstrapper] Missing EnemyData for id(s):\n- " +
                string.Join("\n- ", missingEnemyIds) +
                "\nFix: Rebuild CombatContentIndex (Tools -> Combat -> Build Content Index) and ensure IDs were assigned."
            );
            return; // fail-fast to avoid NRE
        }

        EnemySystem.Instance.Setup(enemies);

        // Resolve Deck
        var deck = new List<CardData>(snapshot.deckCardIds.Count);
        var missingCardIds = new List<string>();

        foreach (var cardId in snapshot.deckCardIds)
        {
            var card = cardDatabase.GetById(cardId);
            if (card == null)
                missingCardIds.Add(cardId);
            else
                deck.Add(card);
        }

        if (missingCardIds.Count > 0)
        {
            Log.Error(LogArea.Combat, () => 
                "[CombatBootstrapper] Missing CardData for id(s):\n- " +
                string.Join("\n- ", missingCardIds) +
                "\nFix: Ensure CardDatabase contains these cards and their Ids match the snapshot."
            );
            return;
        }

        CardSystem.Instance.Setup(deck);

        // Optional: Perk scene-default
        if (perkData != null)
            PerkSystem.Instance.AddPerk(new Perk(perkData));

        // Start hand
        ActionSystem.Instance.Perform(new DrawCardsGA(snapshot.handSize));
    }
}
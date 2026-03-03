using Game.Logging;
using Game.Scenes.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CombatSessionBootstrapper : MonoBehaviour
{
    [Header("Content Lookup")]
    [SerializeField] private CardDatabaseSO cardDatabase;
    [SerializeField] private CombatContentIndex contentIndex;

    [Header("Optional Scene Defaults")]
    [SerializeField] private PerkData perkData;

    private CombatContentRegistry registry;

    // Convention in your project: Hero = 0, enemies = 1..n
    private static readonly CombatantId HeroId = new CombatantId(0);

    private void Awake() => BuildRegistry();

    public void StartCombat(CombatSetupSnapshot snapshot)
    {
        if (!TryCreateResolvedSetup(snapshot, out var setup))
            return;

        var state = CreateInitialState(setup);

        InitializeSession(setup, state);
        SpawnCombatants(setup);
        SetupDeck(setup);

        ApplyOptionalPerk();
        StartOpeningHand(setup);
    }

    // ---------------------------
    // Pipeline steps
    // ---------------------------

    private void BuildRegistry()
    {
        if (cardDatabase == null)
            Log.Error(LogArea.Combat, () => "[CombatSessionBootstrapper] CardDatabase reference is missing.");

        if (contentIndex == null)
        {
            Log.Error(LogArea.Combat, () => "[CombatSessionBootstrapper] CombatContentIndex reference is missing.");
            registry = null;
            return;
        }

        registry = new CombatContentRegistry(contentIndex);
    }

    private bool TryCreateResolvedSetup(CombatSetupSnapshot snapshot, out ResolvedCombatSetup setup)
    {
        setup = null;

        if (!RequireRegistry())
            return false;

        if (!CombatSetupResolver.TryResolve(snapshot, registry, cardDatabase, out setup, out var error))
        {
            Log.Error(LogArea.Combat, () => error);
            return false;
        }

        return true;
    }

    private static CombatState CreateInitialState(ResolvedCombatSetup setup)
        => CombatStateFactory.CreateFromData(HeroId, setup.Hero, setup.Enemies);

    private static void InitializeSession(ResolvedCombatSetup setup, CombatState state)
    {
        // Single source of truth
        CombatContextService.Instance.Initialize(setup.Snapshot, state);
    }

    private static void SpawnCombatants(ResolvedCombatSetup setup)
    {
        // IDs must match the state (Hero = 0)
        HeroSystem.Instance.Setup(HeroId, setup.Hero);

        // Enemies deterministic 1..n (EnemySystem uses board order)
        EnemySystem.Instance.Setup(new List<EnemyData>(setup.Enemies));
    }

    private static void SetupDeck(ResolvedCombatSetup setup)
    {
        CardSystem.Instance.Setup(new List<CardData>(setup.Deck));
    }

    private void ApplyOptionalPerk()
    {
        if (perkData != null)
            PerkService.Instance.AddPerk(new Perk(perkData));
    }

    private static void StartOpeningHand(ResolvedCombatSetup setup)
    {
        ActionSystem.Instance.Perform(new DrawCardsGA(setup.Snapshot.handSize));
    }

    private bool RequireRegistry()
    {
        if (registry != null)
            return true;

        Log.Error(LogArea.Combat, () =>
            "[CombatSessionBootstrapper] Content registry is not built (missing contentIndex or Awake not executed?).");
        return false;
    }

    // ---------------------------
    // Helpers (same file, no partial)
    // ---------------------------

    private sealed class ResolvedCombatSetup
    {
        public CombatSetupSnapshot Snapshot { get; }
        public HeroData Hero { get; }
        public IReadOnlyList<EnemyData> Enemies { get; }
        public IReadOnlyList<CardData> Deck { get; }
        public EncounterDefinition Encounter { get; }

        public ResolvedCombatSetup(
            CombatSetupSnapshot snapshot,
            HeroData hero,
            List<EnemyData> enemies,
            List<CardData> deck,
            EncounterDefinition encounter)
        {
            Snapshot = snapshot;
            Hero = hero;
            Enemies = enemies;
            Deck = deck;
            Encounter = encounter;
        }
    }

    private sealed class CombatContentRegistry
    {
        private readonly Dictionary<string, EnemyData> enemiesById;
        private readonly Dictionary<string, EncounterDefinition> encountersById;
        private readonly Dictionary<int, HeroData> heroesById;

        public CombatContentRegistry(CombatContentIndex index)
        {
            enemiesById = BuildEnemyIndex(index);
            encountersById = BuildEncounterIndex(index);
            heroesById = BuildHeroIndex(index);

            Log.Info(LogArea.Combat, () =>
                $"[CombatContentRegistry] Index built. Enemies={enemiesById.Count}, Encounters={encountersById.Count}, Heroes={heroesById.Count}");
        }

        public bool TryGetHero(int heroId, out HeroData hero)
            => heroesById.TryGetValue(heroId, out hero) && hero != null;

        public bool TryGetEnemy(string enemyId, out EnemyData enemy)
            => enemiesById.TryGetValue(enemyId, out enemy) && enemy != null;

        public bool TryGetEncounter(string encounterId, out EncounterDefinition enc)
            => encountersById.TryGetValue(encounterId, out enc) && enc != null;

        private static Dictionary<string, EnemyData> BuildEnemyIndex(CombatContentIndex index)
        {
            var map = new Dictionary<string, EnemyData>(StringComparer.Ordinal);
            if (index == null) return map;

            foreach (var e in index.enemies)
            {
                if (e == null) continue;

                if (string.IsNullOrWhiteSpace(e.Id))
                {
                    Log.Error(LogArea.Combat, () => $"[CombatContentRegistry] EnemyData '{e.name}' has empty Id.");
                    continue;
                }

                if (!map.TryAdd(e.Id, e))
                    Log.Warn(LogArea.Combat, () => $"[CombatContentRegistry] Duplicate EnemyData Id '{e.Id}' on '{e.name}'.");
            }

            return map;
        }

        private static Dictionary<string, EncounterDefinition> BuildEncounterIndex(CombatContentIndex index)
        {
            var map = new Dictionary<string, EncounterDefinition>(StringComparer.Ordinal);
            if (index == null) return map;

            foreach (var enc in index.encounters)
            {
                if (enc == null) continue;

                if (string.IsNullOrWhiteSpace(enc.Id))
                {
                    Log.Error(LogArea.Combat, () => $"[CombatContentRegistry] EncounterDefinition '{enc.name}' has empty Id.");
                    continue;
                }

                if (!map.TryAdd(enc.Id, enc))
                    Log.Warn(LogArea.Combat, () => $"[CombatContentRegistry] Duplicate EncounterDefinition Id '{enc.Id}' on '{enc.name}'.");
            }

            return map;
        }

        private static Dictionary<int, HeroData> BuildHeroIndex(CombatContentIndex index)
        {
            var map = new Dictionary<int, HeroData>();
            if (index == null) return map;

            foreach (var h in index.heroes)
            {
                if (h == null) continue;

                var key = (int)h.HeroID;
                if (!map.TryAdd(key, h))
                    Log.Warn(LogArea.Combat, () => $"[CombatContentRegistry] Duplicate HeroID '{h.HeroID}' on '{h.name}'.");
            }

            return map;
        }
    }

    private static class CombatSetupResolver
    {
        public static bool TryResolve(
            CombatSetupSnapshot snapshot,
            CombatContentRegistry registry,
            CardDatabaseSO cardDatabase,
            out ResolvedCombatSetup setup,
            out string error)
        {
            setup = null;
            error = null;

            if (snapshot == null)
            {
                error = "[CombatSetupResolver] Snapshot is null.";
                return false;
            }

            if (registry == null)
            {
                error = "[CombatSetupResolver] Content registry is null.";
                return false;
            }

            if (cardDatabase == null)
            {
                error = "[CombatSetupResolver] CardDatabase is null.";
                return false;
            }

            // Encounter (HARD validation)
            if (string.IsNullOrWhiteSpace(snapshot.encounterId))
            {
                error = "[CombatSetupResolver] Snapshot.encounterId is missing/empty, but encounter is required.";
                return false;
            }

            if (!registry.TryGetEncounter(snapshot.encounterId, out var encounter))
            {
                error =
                    $"[CombatSetupResolver] Missing EncounterDefinition for encounterId='{snapshot.encounterId}'. " +
                    "Fix: Ensure CombatContentIndex contains this encounter id and rebuild the index.";
                return false;
            }

            // Hero
            if (!registry.TryGetHero(snapshot.heroId, out var hero))
            {
                error = $"[CombatSetupResolver] Missing HeroData for heroId={snapshot.heroId}.";
                return false;
            }

            // Enemies
            var enemies = new List<EnemyData>(snapshot.enemyIds.Count);
            var missingEnemyIds = new List<string>();

            foreach (var id in snapshot.enemyIds)
            {
                if (!registry.TryGetEnemy(id, out var enemy))
                    missingEnemyIds.Add(id);
                else
                    enemies.Add(enemy);
            }

            // Deck
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

            if (missingEnemyIds.Count > 0 || missingCardIds.Count > 0)
            {
                error = BuildResolveError(missingEnemyIds, missingCardIds);
                return false;
            }

            setup = new ResolvedCombatSetup(snapshot, hero, enemies, deck, encounter);
            return true;
        }

        private static string BuildResolveError(List<string> missingEnemyIds, List<string> missingCardIds)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[CombatSetupResolver] Failed to resolve snapshot:");

            if (missingEnemyIds.Count > 0)
            {
                sb.AppendLine("Missing EnemyData id(s):");
                foreach (var id in missingEnemyIds) sb.AppendLine($"- {id}");
            }

            if (missingCardIds.Count > 0)
            {
                sb.AppendLine("Missing CardData id(s):");
                foreach (var id in missingCardIds) sb.AppendLine($"- {id}");
            }

            sb.Append("Fix: Rebuild CombatContentIndex and ensure CardDatabase contains all ids used by snapshot.");
            return sb.ToString();
        }
    }

    private static class CombatStateFactory
    {
        public static CombatState CreateFromData(CombatantId heroId, HeroData heroData, IReadOnlyList<EnemyData> enemies)
        {
            var state = new CombatState();

            state.AddCombatant(heroId, new CombatantState(heroData.Health));

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemyId = new CombatantId(i + 1);
                state.AddCombatant(enemyId, new CombatantState(enemies[i].Health));
            }

            return state;
        }
    }
}
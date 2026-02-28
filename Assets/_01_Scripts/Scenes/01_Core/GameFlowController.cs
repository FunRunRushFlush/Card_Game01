using System.Collections;
using UnityEngine;

namespace Game.Scenes.Core
{
    public class GameFlowController : MonoBehaviour
    {
        public static GameFlowController Current { get; private set; }

        [SerializeField] private int startingGold = 0;

        [SerializeField] private BiomeDatabase biomeDb;
        [SerializeField] private string defaultEncounterLevelScene = SceneDatabase.Scenes.Arena_Forest_01;

        private RunState run;
        private string activeLevelScene;

        private bool _isTransitioning;

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
            DontDestroyOnLoad(gameObject);
        }

        // ---------- Public API (Buttons rufen diese Methoden auf) ----------

        public void StartNewRun() => StartLocked(StartNewRunRoutine());

        public void ChooseCombat() => StartLocked(GoToEncounterRoutine(SceneDatabase.Scenes.Combat));
        public void ChooseShop() => StartLocked(GoToShopViewRoutine(SceneDatabase.Scenes.Shop));
        public void ChooseEvent() => StartLocked(GoToSessionViewRoutine(SceneDatabase.Scenes.Event));

        public void CombatWon() => StartLocked(GoToLootRoutine());
        public void CombatLost() => StartLocked(GoToGameOverRoutine());

        public void ShopLeave() => StartLocked(BackToMapAdvanceNodeRoutine());
        public void EventComplete() => StartLocked(BackToMapAdvanceNodeRoutine());

        public void LootPicked()
        {
            if (run == null) return;
            StartLocked(BackToMapAdvanceNodeRoutine());
        }

        public void LootPickedIntoNextCombat()
        {
            if (run == null) return;

            // Wichtig: AdvanceNode nur, wenn die Transition wirklich startet
            StartLocked(LootPickedIntoNextCombatRoutine());
        }

        public void BossWon() => StartLocked(GoToGameOverRoutine());

        public void BackToMainMenu() => StartLocked(BackToMainMenuRoutine());

        public void GoToCurrentNode()
        {
            CacheSessionRefs();
            if (run == null) 
                return;
            if (_isTransitioning) 
                return;

            var node = run.CurrentNode;

            switch (run.CurrentNodeType)
            {
                case MapNodeType.Combat:
                case MapNodeType.EliteCombat:
                case MapNodeType.Boss:
                    StartLocked(GoToEncounterRoutine(SceneDatabase.Scenes.Combat));
                    break;

                case MapNodeType.Shop:
                    {
                        var shopScene = (node != null && !string.IsNullOrWhiteSpace(node.sceneOverride))
                            ? node.sceneOverride
                            : SceneDatabase.Scenes.Shop;

                        StartLocked(GoToShopViewRoutine(shopScene));
                        break;
                    }

                case MapNodeType.Event:
                    {
                        var eventScene = (node != null && !string.IsNullOrWhiteSpace(node.sceneOverride))
                            ? node.sceneOverride
                            : SceneDatabase.Scenes.Event;

                        StartLocked(GoToSessionViewRoutine(eventScene));
                        break;
                    }
            }
        }

        // ---------- Lock-Helper ----------

        private void StartLocked(IEnumerator routine)
        {
            if (_isTransitioning) return;

            _isTransitioning = true;

            StartCoroutine(LockedRoutine(routine));
        }

        private IEnumerator LockedRoutine(IEnumerator routine)
        {
            try
            {
                yield return routine;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        // ---------- Routines ----------

        private IEnumerator StartNewRunRoutine()
        {
            // Load Session + Map (SessionView), unload Menu
            yield return SceneController.Current
                .NewTransition()
                .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
                .Load(SceneDatabase.Slots.SessionView, SceneDatabase.Scenes.Map, setActive: true)
                .Unload(SceneDatabase.Slots.Menu)
                .WithOverlay()
                .Perform();

            CacheSessionRefs();

            var session = CoreManager.Instance.Session;
            var heroData = session?.Hero?.Data;

        
            int startingGold = heroData != null ? heroData.BaseStartingGold : 0;

            run.StartNewRun(startingGold);
        }

        private IEnumerator GoToEncounterRoutine(string encounterScene)
        {
            CacheSessionRefs();
            if (run == null) yield break;

            // Wenn wir auf Boss-Node sind, zwingen wir Boss-Scene (Systems)
            var systemsScene = encounterScene;

            // TODO: hier später sauber über Node/Seed auswählen
            var levelScene = GetArenaSceneForCurrentNode(run);
            activeLevelScene = levelScene;

            yield return SceneController.Current
                .NewTransition()
                .Unload(SceneDatabase.Slots.SessionView)
                .Load(SceneDatabase.Slots.EncounterSystems, systemsScene, setActive: true)
                .Load(SceneDatabase.Slots.EncounterLevel, levelScene)
                .WithOverlay()
                .Perform();
        }

        private IEnumerator GoToShopViewRoutine(string encounterScene)
        {
            CacheSessionRefs();
            if (run == null) yield break;

            var systemsScene = encounterScene;

            // TODO: hier später sauber über Node/Seed auswählen
            var levelScene = GetArenaSceneForCurrentNode(run);
            activeLevelScene = levelScene;

            yield return SceneController.Current
                .NewTransition()
                .Unload(SceneDatabase.Slots.SessionView)
                .Load(SceneDatabase.Slots.EncounterSystems, systemsScene, setActive: true)
                .Load(SceneDatabase.Slots.EncounterLevel, levelScene)
                .WithOverlay()
                .Perform();
        }

        private IEnumerator GoToLootRoutine()
        {
            CacheSessionRefs();
            if (run == null) yield break;

            yield return SceneController.Current
                .NewTransition()
                .Unload(SceneDatabase.Slots.EncounterSystems)
                .Load(SceneDatabase.Slots.SessionView, SceneDatabase.Scenes.Loot, setActive: true)
                .WithOverlay()
                .Perform();
        }

        private IEnumerator BackToMapAdvanceNodeRoutine()
        {
            CacheSessionRefs();
            if (run == null) yield break;

            run.AdvanceNode();

            yield return SceneController.Current
                .NewTransition()
                .Unload(SceneDatabase.Slots.EncounterSystems)
                .Unload(SceneDatabase.Slots.EncounterLevel)
                .Load(SceneDatabase.Slots.SessionView, SceneDatabase.Scenes.Map, setActive: true)
                .WithOverlay()
                .Perform();
        }

        private IEnumerator GoToGameOverRoutine()
        {
            yield return SceneController.Current
                .NewTransition()
                .Unload(SceneDatabase.Slots.EncounterSystems)
                .Unload(SceneDatabase.Slots.EncounterLevel)
                .Load(SceneDatabase.Slots.SessionView, SceneDatabase.Scenes.GameOver, setActive: true)
                .Perform();
        }

        private IEnumerator GoToSessionViewRoutine(string scene)
        {
            yield return SceneController.Current
                .NewTransition()
                .Unload(SceneDatabase.Slots.EncounterSystems)
                .Unload(SceneDatabase.Slots.EncounterLevel)
                .Load(SceneDatabase.Slots.SessionView, scene, setActive: true)
                .WithOverlay()
                .Perform();
        }

        private IEnumerator BackToMainMenuRoutine()
        {
            yield return SceneController.Current
                .NewTransition()
                .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true)
                .Unload(SceneDatabase.Slots.SessionView)
                .Unload(SceneDatabase.Slots.EncounterSystems)
                .Unload(SceneDatabase.Slots.EncounterLevel)
                .Unload(SceneDatabase.Slots.Session)
                .WithClearUnusedAssets()
                .WithOverlay()
                .Perform();

            run = null;
        }

        private IEnumerator LootPickedIntoNextCombatRoutine()
        {
            CacheSessionRefs();
            if (run == null) yield break;

            run.AdvanceNode();

            yield return GoToEncounterRoutine(SceneDatabase.Scenes.Combat);
        }

        private void CacheSessionRefs()
        {
            var session = CoreManager.Instance?.Session;
            if (session == null) return;

            run = session.Run;
        }

        private string GetArenaSceneForCurrentNode(RunState run)
        {
            var node = run.CurrentNode;
            if (node != null && !string.IsNullOrWhiteSpace(node.levelSceneOverride))
                return node.levelSceneOverride;

            var biomeDef = biomeDb.Get(run.CurrentBiome);
            if (biomeDef == null) return defaultEncounterLevelScene;

            if (run.CurrentNodeType == MapNodeType.Boss)
                return biomeDef.bossArenaScene;

            var scenes = run.CurrentNodeType == MapNodeType.EliteCombat
                ? biomeDef.eliteArenaScenes
                : biomeDef.normalArenaScenes;

            if (scenes == null || scenes.Length == 0) return defaultEncounterLevelScene;

            var rng = run.CreateNodeRng(salt: 4242);
            return scenes[rng.Next(0, scenes.Length)];
        }
    }
}
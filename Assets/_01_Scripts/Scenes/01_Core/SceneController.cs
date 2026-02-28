using Game.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scenes.Core
{
    public class SceneController : MonoBehaviour, ISceneController
    {
        #region SingeltonInit
        public static ISceneController Current { get; private set; }

        void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }
            Current = this;
        }
        public void Load(string sceneName)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        [SerializeField] private LoadingOverlay loadingOverlay;
        private Dictionary<string, string> loadedSceneBySlot = new();
        private bool isBusy = false;


        public SceneTransitionPlan NewTransition()
        {
            return new SceneTransitionPlan();
        }

        public Coroutine ExecutePlan(SceneTransitionPlan plan)
        {
            if (isBusy)
            {
                Log.Warn(LogCat.General, () => "Scene change already in progress!", this);
                return null;
            }
            isBusy = true;
            return StartCoroutine(ChangeSceneRoutine(plan));
        }

        private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
        {
            Log.Info(LogCat.General, () => $"{nameof(ChangeSceneRoutine)}: {plan.ActiveSceneName}", this);
            if (plan.Overlay)
            {
                yield return loadingOverlay.FadeInBlack();
                yield return new WaitForSeconds(0.5f);
            }
            foreach (var slotKey in plan.ScenesToUnload)
            {
                yield return UnloadSceneRoutine(slotKey);
            }
            if (plan.ClearUnusedAssets)
            {
                yield return CleanupUnusedAssetsRoutine();
            }
            foreach (var kvp in plan.ScenesToLoad)
            {
                if (loadedSceneBySlot.ContainsKey(kvp.Key))
                {
                    yield return UnloadSceneRoutine(kvp.Key);
                }
                yield return LoadingAdditiveRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);
            }
            if (plan.Overlay)
            {
                yield return loadingOverlay.FadeOutBlack();
            }
            isBusy = false;

        }

        private IEnumerator CleanupUnusedAssetsRoutine()
        {
            AsyncOperation cleanupOp = Resources.UnloadUnusedAssets();
            while (!cleanupOp.isDone)
            {
                yield return null;
            }

        }

        private IEnumerator UnloadSceneRoutine(string slotKey)
        {
            if (!loadedSceneBySlot.TryGetValue(slotKey, out string sceneName))
            {
                yield break;
            }
            if (string.IsNullOrEmpty(sceneName))
            {
                yield break;
            }
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
            if (unloadOp != null)
            {
                while (!unloadOp.isDone)
                {
                    yield return null;
                }
            }
            loadedSceneBySlot.Remove(slotKey);
        }

        private IEnumerator LoadingAdditiveRoutine(string slotKey, string sceneName, bool setActive)
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                yield break;
            }
            while (loadOp.progress < 0.9f)
            {
                yield return null;
            }

            loadOp.allowSceneActivation = true;
            while (!loadOp.isDone)
            {
                yield return null;
            }

            if (setActive)
            {
                Scene newScene = SceneManager.GetSceneByName(sceneName);
                if (newScene.IsValid() && newScene.isLoaded)
                {
                    SceneManager.SetActiveScene(newScene);
                }
            }
            loadedSceneBySlot[slotKey] = sceneName;
        }

        public class SceneTransitionPlan
        {
            public Dictionary<string, string> ScenesToLoad { get; } = new();
            public List<string> ScenesToUnload { get; } = new();
            public string ActiveSceneName { get; private set; } = "";
            public bool ClearUnusedAssets { get; private set; } = false;

            public bool Overlay { get; private set; } = false;

            public SceneTransitionPlan Load(string slotKey, string sceneName, bool setActive = false)
            {
                ScenesToLoad[slotKey] = sceneName;
                if (setActive)
                {
                    ActiveSceneName = sceneName;
                }
                return this;
            }
            public SceneTransitionPlan Unload(string slotKey)
            {
                ScenesToUnload.Add(slotKey);
                return this;
            }
            public SceneTransitionPlan WithOverlay()
            {
                Overlay = true;
                return this;
            }
            public SceneTransitionPlan WithClearUnusedAssets()
            {
                ClearUnusedAssets = true;
                return this;
            }

            public Coroutine Perform()
            {
                return SceneController.Current.ExecutePlan(this);
            }
        }

    }
}
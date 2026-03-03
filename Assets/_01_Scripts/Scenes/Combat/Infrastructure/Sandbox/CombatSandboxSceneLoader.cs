using Game.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatSandboxSceneLoader : MonoBehaviour
{
    [Header("Which scene contains the real Combat stack?")]
    [SerializeField] private string combatSceneName = "Combat";

    [Header("Behavior")]
    [SerializeField] private bool autoLoadLastOnPlay = false;

    private readonly List<string> snapshotPaths = new();
    private int selectedIndex = 0;

    // Start-Guard
    private bool started;

    private string FolderPath => CombatSnapshotIO.FolderPath;
    private string LastPath => Path.Combine(FolderPath, "Last.json");

    private void Awake()
    {
        RefreshSnapshotList();
    }

    private void Start()
    {
        if (autoLoadLastOnPlay)
            LoadLastAndStart();
    }

    [ContextMenu("Refresh Snapshot List")]
    public void RefreshSnapshotList()
    {
        snapshotPaths.Clear();
        Directory.CreateDirectory(FolderPath);

        var files = Directory.GetFiles(FolderPath, "*.json", SearchOption.TopDirectoryOnly);

        var timestamped = new List<string>();
        string last = null;

        foreach (var f in files)
        {
            if (string.Equals(Path.GetFileName(f), "Last.json", StringComparison.OrdinalIgnoreCase))
                last = f;
            else
                timestamped.Add(f);
        }

        timestamped.Sort((a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

        snapshotPaths.AddRange(timestamped);
        if (last != null) snapshotPaths.Add(last);

        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, snapshotPaths.Count - 1));
        Log.Info(LogArea.Sandbox, () => $"Found {snapshotPaths.Count} snapshot(s) in: {FolderPath}", this);
    }

    public void LoadLastAndStart()
    {
        if (!ValidateNotStarted()) return;

        if (!File.Exists(LastPath))
        {
            Log.Error(LogArea.Sandbox, () =>
                $"Last.json not found: {LastPath}\nStart a normal combat once to generate it.", this);
            return;
        }

        StartCoroutine(LoadCombatSceneAndStart(LastPath));
    }

    public void LoadSelectedAndStart()
    {
        if (!ValidateNotStarted()) return;

        if (snapshotPaths.Count == 0)
        {
            Log.Error(LogArea.Sandbox, () => "No snapshots found. Press Refresh first.", this);
            return;
        }

        var path = snapshotPaths[Mathf.Clamp(selectedIndex, 0, snapshotPaths.Count - 1)];
        if (!File.Exists(path))
        {
            Log.Error(LogArea.Sandbox, () => $"Snapshot missing: {path}", this);
            return;
        }

        StartCoroutine(LoadCombatSceneAndStart(path));
    }

    public void ReloadSandbox()
    {
        // Optional: reset flag so the next run can start again
        CombatSandboxMode.IsActive = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CopySnapshotsFolderPath()
    {
        GUIUtility.systemCopyBuffer = FolderPath;
        Log.Info(LogArea.Sandbox, () => $"Copied snapshots folder path to clipboard:\n{FolderPath}", this);
    }

    private IEnumerator LoadCombatSceneAndStart(string snapshotPath)
    {
        // Important: prevent MatchSetupSystem auto-start in Combat scene
        CombatSandboxMode.IsActive = true;

        // Load snapshot first (fail early if JSON is broken)
        CombatSetupSnapshot snapshot;
        try
        {
            snapshot = CombatSnapshotIO.Load(snapshotPath);
        }
        catch (Exception e)
        {
            Log.Error(LogArea.Sandbox, () =>
                $"Failed to load snapshot '{snapshotPath}'. Exception:\n{e}", this);
            yield break;
        }

        if (snapshot == null)
        {
            Log.Error(LogArea.Sandbox, () => $"Snapshot loaded as null: {snapshotPath}", this);
            yield break;
        }

        // Load real Combat scene additively if not loaded yet
        if (!IsSceneLoaded(combatSceneName))
        {
            Log.Info(LogArea.Sandbox, () => $"Loading scene '{combatSceneName}' additively...", this);
            var op = SceneManager.LoadSceneAsync(combatSceneName, LoadSceneMode.Additive);
            if (op == null)
            {
                Log.Error(LogArea.Sandbox, () =>
                    $"LoadSceneAsync returned null for '{combatSceneName}'. Is it in Build Settings?", this);
                yield break;
            }

            while (!op.isDone)
                yield return null;

            Log.Info(LogArea.Sandbox, () => $"Scene '{combatSceneName}' loaded.", this);
        }

        // Find bootstrapper
        var bootstrapper = FindFirstObjectByType<CombatSessionBootstrapper>();
        if (bootstrapper == null)
        {
            Log.Error(LogArea.Sandbox, () =>
                "Could not find CombatBootstrapper in loaded Combat scene.", this);
            yield break;
        }

        // Optional dependency check (nice error instead of NRE)
        if (CardViewCreator.Instance == null)
        {
            Log.Error(LogArea.Sandbox, () =>
                "CardViewCreator.Instance is null. Ensure it exists in the Combat scene.", this);
            yield break;
        }

        Log.Info(LogArea.Sandbox, () =>
            $"Starting from '{Path.GetFileName(snapshotPath)}' (seed={snapshot.seed}, encounter={snapshot.encounterId})", this);

        started = true;
        bootstrapper.StartCombat(snapshot);
    }

    private bool ValidateNotStarted()
    {
        if (started)
        {
            Log.Warn(LogArea.Sandbox, () =>
                "Combat already started. Reload Sandbox to start again.", this);
            return false;
        }
        return true;
    }

    private static bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && string.Equals(s.name, sceneName, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private void OnGUI()
    {
        const int w = 640;
        const int h = 370;

        GUILayout.BeginArea(new Rect(10, 10, w, h), GUI.skin.box);
        GUILayout.Label("<b>Combat Sandbox (loads real Combat scene)</b>", new GUIStyle(GUI.skin.label) { richText = true });

        GUILayout.Label($"Combat scene: {combatSceneName}");
        GUILayout.Label($"Snapshots folder: {FolderPath}");
        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(120))) RefreshSnapshotList();
        if (GUILayout.Button("Load Last.json", GUILayout.Width(160))) LoadLastAndStart();
        if (GUILayout.Button("Copy Snapshots Path", GUILayout.Width(180))) CopySnapshotsFolderPath();
        if (GUILayout.Button("Reload Sandbox", GUILayout.Width(150))) ReloadSandbox();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (snapshotPaths.Count == 0)
        {
            GUILayout.Label("No snapshots found yet.\nStart a normal combat once to generate snapshots.");
        }
        else
        {
            GUILayout.Label("Select snapshot:");
            selectedIndex = Mathf.Clamp(selectedIndex, 0, snapshotPaths.Count - 1);

            int start = Mathf.Max(0, selectedIndex - 5);
            int end = Mathf.Min(snapshotPaths.Count, start + 10);

            for (int i = start; i < end; i++)
            {
                var name = Path.GetFileName(snapshotPaths[i]);
                bool isSelected = i == selectedIndex;

                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)))
                    selectedIndex = i;

                GUILayout.Label(name);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(6);
            if (GUILayout.Button("Load Selected & Start", GUILayout.Width(220)))
                LoadSelectedAndStart();
        }

        GUILayout.EndArea();
    }
}
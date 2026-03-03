using System;
using System.IO;
using UnityEngine;
using Game.Logging;

public static class CombatSnapshotIO
{
    public static string FolderPath =>
        Path.Combine(Application.persistentDataPath, "combat_snapshots");

    public static (string lastPath, string timestampPath) SaveAlways(CombatSetupSnapshot snapshot)
    {
        Directory.CreateDirectory(FolderPath);

        var json = JsonUtility.ToJson(snapshot, prettyPrint: true);


        Log.Debug(LogArea.Combat, () => $"[CombatSnapshot] Snapshot JSON:\n{json}");


        // Last.json (immer überschreiben)
        var lastPath = Path.Combine(FolderPath, "Last.json");
        File.WriteAllText(lastPath, json);

        // Timestamped copy
        var safeTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var timestampPath = Path.Combine(FolderPath, $"{safeTimestamp}.json");
        File.WriteAllText(timestampPath, json);

        Log.Info(LogArea.Combat, () => $"[CombatSnapshot] Saved: {lastPath}");
        Log.Info(LogArea.Combat, () => $"[CombatSnapshot] Saved: {timestampPath}");

        return (lastPath, timestampPath);
    }

    public static CombatSetupSnapshot Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonUtility.FromJson<CombatSetupSnapshot>(json);
    }
}
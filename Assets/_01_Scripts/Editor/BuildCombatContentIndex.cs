using Game.Logging;
using UnityEditor;
using UnityEngine;

public static class BuildCombatContentIndex
{
    [MenuItem("Tools/Combat/Build Content Index (selected asset)")]
    public static void BuildSelected()
    {
        var idx = Selection.activeObject as CombatContentIndex;
        if (idx == null)
        {
            Log.Error(LogArea.Editor, () => "Please select a CombatContentIndex asset in the Project window.");
            return;
        }

        idx.enemies.Clear();
        idx.encounters.Clear();

        // Find all EnemyData
        foreach (var guid in AssetDatabase.FindAssets("t:EnemyData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (asset != null) idx.enemies.Add(asset);
        }

        // Find all EncounterDefinition
        foreach (var guid in AssetDatabase.FindAssets("t:EncounterDefinition"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<EncounterDefinition>(path);
            if (asset != null) idx.encounters.Add(asset);
        }

        // Find all HeroData
        foreach (var guid in AssetDatabase.FindAssets("t:HeroData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<HeroData>(path);
            if (asset != null) idx.heroes.Add(asset);
        }

        EditorUtility.SetDirty(idx);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Log.Info(LogArea.Editor, () => $"Done. Enemies={idx.enemies.Count}, Encounters={idx.encounters.Count}");
    }
}
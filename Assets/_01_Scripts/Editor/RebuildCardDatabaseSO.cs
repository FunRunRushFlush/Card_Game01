using Game.Logging;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class RebuildCardDatabaseSO
{
    [MenuItem("Tools/Cards/Rebuild CardDatabaseSO (selected asset)")]
    public static void RebuildSelected()
    {
        var db = Selection.activeObject as CardDatabaseSO;
        if (db == null)
        {
            Log.Error(LogArea.Editor, () => "Please select a CardDatabaseSO asset in the Project window.");
            return;
        }

        var cards = new List<CardData>();
        foreach (var guid in AssetDatabase.FindAssets("t:CardData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (asset != null) cards.Add(asset);
        }

        // Optional: sort by name for deterministic ordering in inspector
        cards.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        Undo.RecordObject(db, "Rebuild CardDatabaseSO");
        db.Editor_SetCards(cards);
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Log.Info(LogArea.Editor, () => $"Done. Cards={cards.Count}");
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ValidateStableIds
{
    [MenuItem("Tools/IDs/Validate IDs (EnemyData + EncounterDefinition)")]
    public static void Validate()
    {
        ValidateType("EnemyData");
        ValidateType("EncounterDefinition");
    }

    private static void ValidateType(string typeName)
    {
        var guids = AssetDatabase.FindAssets($"t:{typeName}");
        var seen = new Dictionary<string, string>();
        int missing = 0;
        int duplicates = 0;

        foreach (var assetGuid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(assetGuid);
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var idProp = so.FindProperty("id") ?? so.FindProperty("<Id>k__BackingField");

            var id = idProp != null ? idProp.stringValue : null;

            if (string.IsNullOrWhiteSpace(id))
            {
                missing++;
                Debug.LogWarning($"[ValidateStableIds] Missing ID: {typeName} at {path}", obj);
                continue;
            }

            if (seen.TryGetValue(id, out var otherPath))
            {
                duplicates++;
                Debug.LogError($"[ValidateStableIds] Duplicate ID '{id}'\n- {otherPath}\n- {path}", obj);
                continue;
            }

            seen[id] = path;
        }

        Debug.Log($"[ValidateStableIds] {typeName}: checked={guids.Length}, missing={missing}, duplicates={duplicates}");
    }
}
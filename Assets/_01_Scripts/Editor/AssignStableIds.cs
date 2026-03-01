using System;
using UnityEditor;
using UnityEngine;

public static class AssignStableIds
{
    [MenuItem("Tools/IDs/Assign IDs from Asset GUID (EnemyData + EncounterDefinition + CardData)")]
    public static void AssignIdsFromAssetGuid()
    {
        int changed = 0;

        changed += AssignForType(nameof(EnemyData));
        changed += AssignForType(nameof(EncounterDefinition));
        changed += AssignForType(nameof(CardData));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[AssignStableIds] Done. Updated assets: {changed}");
    }

    private static int AssignForType(string typeName)
    {
        int changed = 0;

        var guids = AssetDatabase.FindAssets($"t:{typeName}");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);

            // supports both:
            // 1) [SerializeField] private string id;
            // 2) [field: SerializeField] public string Id {get; private set;} -> "<Id>k__BackingField"
            var idProp = so.FindProperty("id") ?? so.FindProperty("<Id>k__BackingField");
            if (idProp == null || idProp.propertyType != SerializedPropertyType.String)
            {
                Debug.LogWarning($"[AssignStableIds] Could not find id field on {typeName}: {path}");
                continue;
            }

            if (!string.IsNullOrWhiteSpace(idProp.stringValue))
                continue;

            idProp.stringValue = guid; // Asset GUID as stable ID
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(obj);
            changed++;
        }

        Debug.Log($"[AssignStableIds] {typeName}: set IDs on {changed} assets.");
        return changed;
    }
}
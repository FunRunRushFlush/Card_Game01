#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class FindUnusedDataAssets
{
    // Passe das an deinen tats‰chlichen Ordner an:
    private const string RootFolder = "Assets/Data";

    [MenuItem("Tools/Data/Find Unused ScriptableObjects (by dependencies)")]
    public static void Run()
    {
        // 1) Kandidaten: alle ScriptableObjects unter RootFolder
        var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { RootFolder });
        var soPaths = guids.Select(AssetDatabase.GUIDToAssetPath).Distinct().ToList();

        // 2) Referenz-Index: alle Assets im Projekt (Scenes, Prefabs, ScriptableObjects, Materials, etc.)
        //    Wenn das Projekt groﬂ ist, kannst du hier filtern (z.B. nur Scenes+Prefabs).
        var allAssetPaths = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets/"))
            .Where(p => !p.StartsWith("Assets/Editor/"))
            .ToList();

        // Build reverse dependency map: dependency -> set(of owners)
        var ownersByDependency = new Dictionary<string, HashSet<string>>();

        foreach (var owner in allAssetPaths)
        {
            // true = recursive dependencies
            var deps = AssetDatabase.GetDependencies(owner, true);
            foreach (var dep in deps)
            {
                if (!ownersByDependency.TryGetValue(dep, out var set))
                {
                    set = new HashSet<string>();
                    ownersByDependency[dep] = set;
                }
                set.Add(owner);
            }
        }

        // 3) Unused: hat keine Owner auﬂer sich selbst
        var unused = new List<string>();
        foreach (var so in soPaths)
        {
            if (!ownersByDependency.TryGetValue(so, out var owners))
            {
                unused.Add(so);
                continue;
            }

            // Wenn nur "ich selbst" als Dependency auftauche, z‰hlt es als unreferenziert
            var ownersWithoutSelf = owners.Where(o => o != so).ToList();
            if (ownersWithoutSelf.Count == 0)
                unused.Add(so);
        }

        Debug.Log($"[Unused SOs in {RootFolder}] {unused.Count}\n" + string.Join("\n", unused));
        Selection.objects = unused.Select(p => AssetDatabase.LoadMainAssetAtPath(p)).Where(o => o != null).ToArray();
    }
}
#endif
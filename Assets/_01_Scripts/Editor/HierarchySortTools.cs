// Assets/Editor/HierarchySortTools.cs
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class HierarchySortTools
{
    [MenuItem("Tools/Hierarchy/Sort Selected Siblings By Name")]
    private static void SortSelectedSiblingsByName()
    {
        var transforms = Selection.transforms;
        if (transforms == null || transforms.Length == 0) return;

        // Group by parent (null parent = root objects)
        foreach (var group in transforms.GroupBy(t => t.parent))
        {
            var parent = group.Key;

            Transform[] siblings;
            if (parent == null)
            {
                // Root objects
                siblings = transforms
                    .Where(t => t.parent == null)
                    .OrderBy(t => t.name)
                    .ToArray();
            }
            else
            {
                // All children of the parent (sort full set, not just selection)
                siblings = parent.Cast<Transform>()
                    .OrderBy(t => t.name)
                    .ToArray();
            }

            Undo.RecordObjects(siblings, "Sort By Name");
            for (int i = 0; i < siblings.Length; i++)
                siblings[i].SetSiblingIndex(i);
        }
    }

    [MenuItem("Tools/Hierarchy/Sort Children Of Selected By Name")]
    private static void SortChildrenOfSelectedByName()
    {
        var parent = Selection.activeTransform;
        if (parent == null) return;

        var children = parent.Cast<Transform>()
            .OrderBy(t => t.name)
            .ToArray();

        Undo.RecordObjects(children, "Sort Children By Name");
        for (int i = 0; i < children.Length; i++)
            children[i].SetSiblingIndex(i);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtenisons
{
    public static T Draw<T>(this List<T> list)
    {
        if (list.Count == 0)
        {
            Debug.LogWarning($"[{nameof(ListExtenisons)}]Cannot draw from an empty list.");
            return default;
        }
        int lastIndex = list.Count - 1;
        T item = list[lastIndex];
        list.RemoveAt(lastIndex);
        return item;
    }

    /// <summary>
    /// Shuffles the list in-place using Fisher–Yates.
    /// Uses UnityEngine.Random (non-deterministic unless you manage Random.state / InitState).
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1); // upper bound exclusive
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Deterministic shuffle variant (good for seeds, replays, tests).
    /// </summary>
    public static void Shuffle<T>(this IList<T> list, System.Random rng)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        if (rng == null) throw new ArgumentNullException(nameof(rng));

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1); // upper bound exclusive
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

}
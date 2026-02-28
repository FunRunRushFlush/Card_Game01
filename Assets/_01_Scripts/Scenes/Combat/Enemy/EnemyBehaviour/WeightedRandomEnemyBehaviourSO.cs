using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyBehaviour/WeightedRandom")]
public class WeightedRandomEnemyBehaviourSO : EnemyBehaviourSO
{
    [Serializable]
    public class Entry
    {
        public EnemyMoveSO move;

        [Min(0)] public int weight = 1;

        [Tooltip("After selecting this move, it cannot be picked for X turns.")]
        [Min(0)] public int cooldownTurns = 0;

        [Tooltip("If > 0, this move cannot be picked if it was used within the last N moves (history).")]
        [Min(0)] public int forbidRepeatLastN = 0;

        [Tooltip("0 = no limit. 1 = cannot be used twice in a row. 2 = max twice in a row, etc.")]
        [Min(0)] public int maxConsecutive = 0;
    }

    [SerializeField] private List<Entry> entries = new();

    public override EnemyMoveSO PickNextMove(EnemyAIState state, EnemyView enemy)
    {
        if (entries == null || entries.Count == 0)
            return null;

        // 1) Collect candidates with all rules enabled
        var candidates = CollectCandidates(state, includeMaxConsecutiveRule: true);

        // 2) Fallback: if everything is blocked, relax ONLY the consecutive rule first
        if (candidates.Count == 0)
            candidates = CollectCandidates(state, includeMaxConsecutiveRule: false);

        // 3) Final fallback: if still empty, allow anything (avoid soft-lock)
        if (candidates.Count == 0)
            candidates.AddRange(entries);

        // 4) Weighted roll
        int total = 0;
        foreach (var c in candidates)
            total += Mathf.Max(0, c.weight);

        int roll = state.Rng.Next(Mathf.Max(1, total));

        foreach (var c in candidates)
        {
            roll -= Mathf.Max(0, c.weight);
            if (roll < 0)
            {
                state.PutOnCooldown(c.move, c.cooldownTurns);
                return c.move;
            }
        }

        return candidates[0].move;
    }

    private List<Entry> CollectCandidates(EnemyAIState state, bool includeMaxConsecutiveRule)
    {
        var result = new List<Entry>();

        foreach (var e in entries)
        {
            if (e.move == null || e.weight <= 0)
                continue;

            // Cooldown rule
            if (state.IsOnCooldown(e.move))
                continue;

            // History rule (recently used in last N moves)
            if (e.forbidRepeatLastN > 0 && state.WasUsedRecently(e.move, e.forbidRepeatLastN))
                continue;

            // Consecutive rule (no more than N times in a row)
            if (includeMaxConsecutiveRule && e.maxConsecutive > 0)
            {
                if (state.GetConsecutiveCount(e.move) >= e.maxConsecutive)
                    continue;
            }

            result.Add(e);
        }

        return result;
    }
}
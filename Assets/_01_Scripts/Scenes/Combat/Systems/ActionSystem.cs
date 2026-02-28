using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = null;

    public bool IsPerforming { get; private set; } = false;

    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

    private static readonly Dictionary<(Type actionType, ReactionTiming timing), Dictionary<Delegate, List<Action<GameAction>>>> wrapperLookup = new();



    [SerializeField] private bool debugTrace = true; // optional im Inspector
    private static int _depth = 0;

    private string Indent => new string(' ', _depth * 2);
    private void Trace(string msg)
    {
        if (!debugTrace) return;
        Debug.Log($"{Indent}[AS] {msg}");
    }

    private sealed class ReactionSubscription : IDisposable
    {
        private readonly Action disposeAction;
        private bool disposed;

        public ReactionSubscription(Action disposeAction)
        {
            this.disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            disposeAction?.Invoke();
        }
    }

    public void Perform(GameAction action, Action onPerformFinished = null)
    {
        Trace($"Perform ROOT: {action.GetType().Name}");

        if (IsPerforming)
        {
            return;
        }

        IsPerforming = true;
        StartCoroutine(FlowWrapper(action, onPerformFinished));
    }
    public void AddReaction(GameAction gameAction)
    {
        if (debugTrace)
            Debug.Log($"{Indent}[AS] AddReaction -> {(gameAction == null ? "NULL" : gameAction.GetType().Name)} (list={(reactions == null ? "null" : reactions.Count.ToString())})");
        reactions?.Add(gameAction);
    }

    private IEnumerator FlowWrapper(GameAction action, Action onPerformFinished)
    {
        _depth = 0;
        Trace($"FlowWrapper START: {action.GetType().Name}");
        try
        {
            yield return Flow(action);
        }
        finally
        {
            Trace($"FlowWrapper END: {action.GetType().Name}");
            IsPerforming = false;
            onPerformFinished?.Invoke();
        }
    }

    private IEnumerator Flow(GameAction action)
    {
        _depth++;
        Trace($"Flow START: {action.GetType().Name}");

        reactions = action.PreReactions;
        Trace($"PRE reactions: {(reactions == null ? 0 : reactions.Count)}");
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        Trace($"PERFORM reactions before performer: {(reactions == null ? 0 : reactions.Count)}");
        yield return PerformPerformer(action);
        Trace($"PERFORM reactions after performer: {(reactions == null ? 0 : reactions.Count)}");
        yield return PerformReactions();

        reactions = action.PostReactions;
        Trace($"POST reactions: {(reactions == null ? 0 : reactions.Count)}");
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        Trace($"Flow END: {action.GetType().Name}");
        _depth--;
    }



    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();
        if (!subs.TryGetValue(type, out var list) || list.Count == 0)
        {
            return;
        }

        // Snapshot to avoid issues if callbacks subscribe/unsubscribe during iteration
        var snapshot = list.ToArray();
        foreach (var sub in snapshot)
        {
            sub?.Invoke(action);
        }
    }

    private IEnumerator PerformReactions()
    {
        var current = reactions;
        if (current == null || current.Count == 0)
            yield break;

        for (int i = 0; i < current.Count; i++)
        {
            var reaction = current[i];
            yield return Flow(reaction);

            // restore because Flow() overwrites `reactions`
            reactions = current;
        }
    }


    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType();
        if (!performers.TryGetValue(type, out var performer))
        {
            Debug.LogWarning($"{Indent}[AS] NO PERFORMER for {type.Name}");
            yield break;
        }

        Trace($"Run performer: {type.Name}");
        yield return performer(action);
    }


    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {

        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);
        if (performers.ContainsKey(type))
        {
            performers[type] = wrappedPerformer;
        }
        else
        {
            performers.Add(type, wrappedPerformer);
        }
    }

    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type))
        {
            performers.Remove(type);
        }
    }

    /// <summary>
    /// Subscribes a reaction callback for a specific GameAction type and timing.
    /// Returns an IDisposable token that can be disposed to unsubscribe safely.
    /// </summary>
    public static IDisposable SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        if (reaction == null) throw new ArgumentNullException(nameof(reaction));

        var subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        var type = typeof(T);

        Action<GameAction> wrappedReaction = (GameAction action) => reaction((T)action);

        if (!subs.TryGetValue(type, out var list))
        {
            list = new List<Action<GameAction>>();
            subs.Add(type, list);
        }
        list.Add(wrappedReaction);

        var key = (type, timing);
        if (!wrapperLookup.TryGetValue(key, out var map))
        {
            map = new Dictionary<Delegate, List<Action<GameAction>>>();
            wrapperLookup.Add(key, map);
        }
        if (!map.TryGetValue(reaction, out var wrappers))
        {
            wrappers = new List<Action<GameAction>>();
            map.Add(reaction, wrappers);
        }
        wrappers.Add(wrappedReaction);

        return new ReactionSubscription(() => UnsubscribeInternal(type, timing, reaction, wrappedReaction));
    }

    /// <summary>
    /// Legacy unsubscribe method (works now). Prefer storing the IDisposable token returned by SubscribeReaction.
    /// </summary>
    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        if (reaction == null) return;

        var type = typeof(T);
        var key = (type, timing);

        if (!wrapperLookup.TryGetValue(key, out var map))
            return;

        if (!map.TryGetValue(reaction, out var wrappers) || wrappers.Count == 0)
            return;

        // remove last wrapper for this delegate
        var wrapper = wrappers[wrappers.Count - 1];
        wrappers.RemoveAt(wrappers.Count - 1);

        if (wrappers.Count == 0)
            map.Remove(reaction);

        if (map.Count == 0)
            wrapperLookup.Remove(key);

        UnsubscribeInternal(type, timing, reaction, wrapper);
    }

    private static void UnsubscribeInternal(Type actionType, ReactionTiming timing, Delegate original, Action<GameAction> wrapper)
    {
        // Remove from subscriber list
        var subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        if (subs.TryGetValue(actionType, out var list))
        {
            list.Remove(wrapper);
            if (list.Count == 0)
                subs.Remove(actionType);
        }

        // Remove from lookup (best effort)
        var key = (actionType, timing);
        if (original != null && wrapperLookup.TryGetValue(key, out var map) && map.TryGetValue(original, out var wrappers))
        {
            wrappers.Remove(wrapper);
            if (wrappers.Count == 0)
                map.Remove(original);
            if (map.Count == 0)
                wrapperLookup.Remove(key);
        }
    }


}
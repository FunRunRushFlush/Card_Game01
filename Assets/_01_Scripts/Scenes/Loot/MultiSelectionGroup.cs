using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiSelectionGroup : MonoBehaviour
{
    [SerializeField] private int maxSelected = 1;
    [SerializeField] private bool allowDeselect = true;

    private readonly List<CardViewUI> all = new();
    private readonly List<CardViewUI> selected = new();

    public IReadOnlyList<CardViewUI> Selected => selected;
    public event Action SelectionChanged;

    public void Register(CardViewUI view)
    {
        if (view == null || all.Contains(view)) return;

        all.Add(view);
        view.Clicked += OnClicked;
        view.SetSelected(false);
    }

    public void Clear()
    {
        foreach (var v in all)
        {
            if (v != null)
            {
                v.Clicked -= OnClicked;
            }
        }

        all.Clear();
        selected.Clear();
        SelectionChanged?.Invoke();
    }


    private void OnDestroy() => Clear();

    private void OnClicked(CardViewUI view)
    {
        if (view == null) return;

        // already selected -> maybe deselect
        if (selected.Contains(view))
        {
            if (!allowDeselect) return;

            selected.Remove(view);
            view.SetSelected(false);
            SelectionChanged?.Invoke();
            return;
        }

        // select new
        if (selected.Count >= maxSelected)
        {
            // Option A: "first selected gets replaced"
            var toRemove = selected[0];
            selected.RemoveAt(0);
            if (toRemove != null) toRemove.SetSelected(false);
        }

        selected.Add(view);
        view.SetSelected(true);
        SelectionChanged?.Invoke();
    }

    public void SetRules(int max, bool canDeselect)
    {
        maxSelected = Mathf.Max(1, max);
        allowDeselect = canDeselect;

        // Optional: falls nach Regelwechsel zu viele selected sind
        while (selected.Count > maxSelected)
        {
            var v = selected[0];
            selected.RemoveAt(0);
            if (v != null) v.SetSelected(false);
        }

        SelectionChanged?.Invoke();
    }
}

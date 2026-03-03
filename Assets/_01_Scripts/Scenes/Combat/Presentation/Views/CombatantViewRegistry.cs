using System.Collections.Generic;
using UnityEngine;

public sealed class CombatantViewRegistry : MonoBehaviour
{
    private readonly Dictionary<CombatantId, CombatantView> _views = new();

    public void Register(CombatantView view) => _views[view.Id] = view;
    public bool TryGet(CombatantId id, out CombatantView view) => _views.TryGetValue(id, out view);
}
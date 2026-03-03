using System.Collections.Generic;

public sealed class CombatantRegistry
{
    private readonly Dictionary<CombatantId, CombatantView> _byId = new();

    public void Register(CombatantView view) => _byId[view.Id] = view;

    public CombatantView Get(CombatantId id) => _byId[id];
    public bool TryGet(CombatantId id, out CombatantView view) => _byId.TryGetValue(id, out view);
}
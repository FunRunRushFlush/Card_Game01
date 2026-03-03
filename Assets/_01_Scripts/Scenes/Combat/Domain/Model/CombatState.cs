using System.Collections.Generic;

public sealed class CombatState
{
    private readonly Dictionary<CombatantId, CombatantState> _combatants = new();

    public CombatantState Get(CombatantId id) => _combatants[id];

    public void AddCombatant(CombatantId id, CombatantState state)
        => _combatants.Add(id, state);

    public bool TryGet(CombatantId id, out CombatantState state)
        => _combatants.TryGetValue(id, out state);
}
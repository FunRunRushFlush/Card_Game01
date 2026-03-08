using System.Collections.Generic;
using UnityEngine;

public class PerkService : Singleton<PerkService>
{
    private readonly List<Perk> _perks = new();

    public IReadOnlyList<Perk> Perks => _perks;

    public void AddPerk(Perk perk)
    {
        _perks.Add(perk);
        perk.OnAdd();

        CombatDomainEventBus.Publish(new PerkAddedEvent(perk));
    }

    public void RemovePerk(Perk perk)
    {
        if (!_perks.Remove(perk))
            return;

        perk.OnRemove();

        CombatDomainEventBus.Publish(new PerkRemovedEvent(perk));
    }
}
using System.Collections.Generic;
using UnityEngine;

public class PerkSystem : Singleton<PerkSystem>
{
    [SerializeField] private PerksUI perksUI;
    private readonly List<Perk> _perks = new List<Perk>();

    public void AddPerk(Perk perk)
    {
        _perks.Add(perk);
        perksUI.AddPerkUI(perk);
        perk.OnAdd();

    }

    public void RemovePerk(Perk perk)
    {
        _perks.Remove(perk);
        perksUI.RemovePerkUI(perk);
        perk.OnRemove();
    }


}

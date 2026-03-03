using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PerksUI : MonoBehaviour
{
    [SerializeField] private PerkUI perkUIPrefab;
    private readonly List<PerkUI> perkUiList = new();

    public void AddPerkUI(Perk perk)
    {
        PerkUI perkUI = Instantiate(perkUIPrefab, transform);
        perkUI.Setup(perk);
        perkUiList.Add(perkUI);

    }

    public void RemovePerkUI(Perk perk)
    {
        PerkUI perkUI = perkUiList.FirstOrDefault(pui => pui.Perk == perk);
        if (perkUI != null)
        {
            perkUiList.Remove(perkUI);
            Destroy(perkUI.gameObject);
        }

    }
}

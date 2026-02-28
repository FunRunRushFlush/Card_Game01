using UnityEngine;
using UnityEngine.UI;

public class PerkUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Image image;
    public Perk Perk { get; private set; }

    public void Setup(Perk perk)
    {
        Perk = perk;
        image.sprite = perk.Image;
    }
}

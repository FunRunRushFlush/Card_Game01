using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Hero")]
public class HeroData : ScriptableObject
{
    [field: SerializeField] public Heros HeroID { get; private set; }

    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int HandSize { get; private set; }

    [field: SerializeField] public int Mana { get; private set; }
    [field: SerializeField] public int BaseStartingGold { get; private set; }

    [field: SerializeField] public List<CardData> Deck { get; private set; }

}

public enum Heros
{
    Hero01,
    Hero02,
    Hero03
}

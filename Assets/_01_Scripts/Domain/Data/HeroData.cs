using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Hero")]
public class HeroData : ScriptableObject
{
    [field: SerializeField] public Heros HeroID { get; private set; }

    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int DrawPerTurn { get; private set; } = 5;
    [field: SerializeField] public int MaxHandSize { get; private set; } = 10;
    [field: SerializeField] public int Mana { get; private set; } = 3;
    [field: SerializeField] public int BaseStartingGold { get; private set; } = 100;

    [field: SerializeField] public List<CardData> Deck { get; private set; }

}

public enum Heros
{
    Hero01,
    Hero02,
    Hero03
}

using System.Collections.Generic;

public sealed class CombatHeroSnapshot
{
    public HeroData HeroTemplate { get; set; }
    public int DrawPerTurn { get; set; }
    public int MaxHandSize { get; set; }
    public int MaxMana { get; set; }
    public int Health { get; set; }                  
    public List<CardData> Deck { get; set; }           
}
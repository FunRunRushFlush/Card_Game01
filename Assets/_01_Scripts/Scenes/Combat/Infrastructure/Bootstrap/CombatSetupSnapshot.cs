using System;
using System.Collections.Generic;

[Serializable]
public class CombatSetupSnapshot
{
    public int version = 1;

    public string createdAtUtc;     // nur Info
    public int seed;

    public int heroId;              // Heros enum als int (robust)
    public int handSize;      // Start-Hand ziehen (kann = drawPerTurn sein)
    public int drawPerTurn;   // pro Turn ziehen
    public int maxHandSize;   // Handlimit
    public int maxMana;       // Mana cap

    public string biome;            // optional (debug)
    public string nodeType;         // optional (debug)
    public int biomeIndex;          // optional (debug)
    public int nodeIndexInBiome;    // optional (debug)

    public string encounterId;
    public List<string> enemyIds = new();

    public List<string> deckCardIds = new();
}
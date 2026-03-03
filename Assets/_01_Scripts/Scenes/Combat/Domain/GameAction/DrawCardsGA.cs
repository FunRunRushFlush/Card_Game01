using Game.Logging;
using UnityEngine;

public class DrawCardsGA : GameAction
{
    public int Amount { get; set; }
    public DrawCardsGA(int amount)
    {
        Log.Debug(LogArea.Combat, () => $"DrawCardGA:  {amount} card(s).");
        Amount = amount;
    }

}

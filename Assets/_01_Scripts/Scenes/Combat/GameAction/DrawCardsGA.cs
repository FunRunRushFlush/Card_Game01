using UnityEngine;

public class DrawCardsGA : GameAction
{
    public int Amount { get; set; }
    public DrawCardsGA(int amount)
    {
        Debug.Log($"DrawCardGA:  {amount} card(s).");
        Amount = amount;
    }

}

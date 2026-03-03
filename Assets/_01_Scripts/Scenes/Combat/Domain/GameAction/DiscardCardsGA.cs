using System.Collections.Generic;

public class DiscardCardsGA : GameAction
{
    public int Amount { get; private set; }

    public DiscardCardsGA(int amount)
    {
        Amount = amount;
    }
}
using System;

public class RoundStateSystem : Singleton<RoundStateSystem>
{
    public int AttacksPlayedThisTurn { get; private set; }

    private IDisposable resetSub;

    private void OnEnable()
    {
        AttacksPlayedThisTurn = 0;

        // Player ends turn => EnemyTurn happens => after enemy turn, new player turn begins
        resetSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnPost, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        resetSub?.Dispose();
        resetSub = null;
    }

    private void OnEnemyTurnPost(EnemyTurnGA _)
    {
        AttacksPlayedThisTurn = 0;
    }

    public void NotifyCardPlayed(Card card)
    {
        if (card != null && card.HasTag(CardTag.Attack))
            AttacksPlayedThisTurn++;
    }
}

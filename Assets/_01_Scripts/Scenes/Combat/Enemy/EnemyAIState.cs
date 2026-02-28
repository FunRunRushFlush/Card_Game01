using System.Collections.Generic;

public class EnemyAIState
{
    public System.Random Rng { get; }
    public EnemyMoveSO CurrentMove { get; private set; }

    public int TurnIndex { get; private set; }
    private readonly Queue<EnemyMoveSO> lastMoves = new();
    private readonly Dictionary<EnemyMoveSO, int> cooldowns = new();



    private EnemyMoveSO lastMove;
    private int lastMoveStreak;



    public EnemyAIState(int seed) => Rng = new System.Random(seed);


    public void SetMove(EnemyMoveSO move)
    {
        CurrentMove = move;


        if (move == null)
        {
            lastMove = null;
            lastMoveStreak = 0;
            return;
        }

        if (move == lastMove)
        {
            lastMoveStreak++;
        } 
        else
        {
            lastMove = move;
            lastMoveStreak = 1;
        }
    }
    public int GetConsecutiveCount(EnemyMoveSO move)
    => (move != null && move == lastMove) ? lastMoveStreak : 0;
    public void AdvanceTurn()
    {
        TurnIndex++;


        var keys = new List<EnemyMoveSO>(cooldowns.Keys);
        foreach (var k in keys)
        {
            cooldowns[k]--;
            if (cooldowns[k] <= 0) 
                cooldowns.Remove(k);
        }

        if (CurrentMove != null)
        {
            lastMoves.Enqueue(CurrentMove);
            while (lastMoves.Count > 3) 
                lastMoves.Dequeue();
        }
    }

    public bool IsOnCooldown(EnemyMoveSO move) => cooldowns.ContainsKey(move);
    public void PutOnCooldown(EnemyMoveSO move, int turns)
    {
        if (turns <= 0) return;
        cooldowns[move] = turns;
    }

    public bool WasUsedRecently(EnemyMoveSO move, int lastN)
    {
        if (lastN <= 0) return false;
        int count = 0;
        foreach (var m in lastMoves)
        {
            if (++count > lastN) break;
            if (m == move) return true;
        }
        return false;
    }
}
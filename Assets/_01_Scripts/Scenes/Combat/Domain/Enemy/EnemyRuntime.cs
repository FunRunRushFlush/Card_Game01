using System.Collections.Generic;

public sealed class EnemyRuntime : IEnemyActor
{
    public CombatantId Id { get; }

    public EnemyData Data { get; }
    public EnemyBehaviourSO Behaviour => Data != null ? Data.Behaviour : null;
    public EnemyAIState AIState { get; }

    public IntentData CurrentIntent { get; private set; }

    public int AttackValue => Data.AttackValue;
    public int MultiAttackValue => Data.MultiAttackValue;
    public int BlockValue => Data.BlockValue;
    public int BurnValue => Data.BurnValue;
    public int PoisonValue => Data.PoisonValue;
    public int StrengthValue => Data.StrengthValue;
    public int WeaknessValue => Data.WeakValue;

    public EnemyRuntime(CombatantId id, EnemyData data, int seed)
    {
        Id = id;
        Data = data;
        AIState = new EnemyAIState(seed);
    }

    public void ChooseNextIntent()
    {
        if (Behaviour == null || AIState == null)
        {
            CurrentIntent = default;
            AIState?.SetMove(null);
            return;
        }

        var move = Behaviour.PickNextMove(AIState, this);
        AIState.SetMove(move);

        CurrentIntent = move != null ? move.GetIntent(this) : default;
    }

    public List<GameAction> BuildActionsFromCurrentIntent()
    {
        var move = AIState?.CurrentMove;
        if (move == null) return new List<GameAction>();
        return move.BuildActions(this) ?? new List<GameAction>();
    }
}
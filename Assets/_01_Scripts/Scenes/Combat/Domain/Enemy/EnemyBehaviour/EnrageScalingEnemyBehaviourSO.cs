using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyBehaviour/EnrageScaling")]
public class EnrageScalingEnemyBehaviourSO : EnemyBehaviourSO
{
    [Header("Base EnemyBehaviour")]
    [SerializeField] private EnemyBehaviourSO baseBehaviour;

    [Header("Forced enrage move")]
    [SerializeField] private EnemyMoveSO enrageMove;

    [Header("Timing")]
    [Min(1)][SerializeField] private int startAtTurn = 3;  
    [Min(1)][SerializeField] private int everyNTurns = 1;  

    public override EnemyMoveSO PickNextMove(EnemyAIState state, EnemyView enemy)
    {
        if (state == null) return null;

        
        int nextTurn = state.TurnIndex + 1;

        bool shouldEnrage =
            enrageMove != null &&
            nextTurn >= startAtTurn &&
            ((nextTurn - startAtTurn) % everyNTurns == 0);

        if (shouldEnrage)
            return enrageMove;

        return baseBehaviour != null
            ? baseBehaviour.PickNextMove(state, enemy)
            : null;
    }
}
using UnityEngine;

public abstract class EnemyBehaviourSO : ScriptableObject
{
    public abstract EnemyMoveSO PickNextMove(EnemyAIState state, EnemyView enemy);
}

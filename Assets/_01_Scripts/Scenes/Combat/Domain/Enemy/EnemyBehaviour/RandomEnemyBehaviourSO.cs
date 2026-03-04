using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyBehaviour/Random")]
public class RandomEnemyBehaviourSO : EnemyBehaviourSO
{
    [SerializeField] private List<EnemyMoveSO> moves = new();

    public override EnemyMoveSO PickNextMove(EnemyAIState state, IEnemyActor enemy)
    {
        if (moves == null || moves.Count == 0 || state == null) return null;
        return moves[state.Rng.Next(moves.Count)];
    }
}
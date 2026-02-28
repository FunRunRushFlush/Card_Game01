using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyBehaviour/Phased")]
public class PhasedEnemyBehaviourSO : EnemyBehaviourSO
{
    [SerializeField] private EnemyBehaviourSO phase1;
    [SerializeField] private EnemyBehaviourSO phase2;
    [Range(0f, 1f)][SerializeField] private float phase2AtHpPercent = 0.5f;

    public override EnemyMoveSO PickNextMove(EnemyAIState state, EnemyView enemy)
    {
        if (enemy == null) return null;

        float hpPct = enemy.MaxHealth <= 0 ? 0 : (enemy.CurrentHealth / (float)enemy.MaxHealth);
        var behaviour = hpPct <= phase2AtHpPercent ? phase2 : phase1;

        return behaviour != null ? behaviour.PickNextMove(state, enemy) : null;
    }
}
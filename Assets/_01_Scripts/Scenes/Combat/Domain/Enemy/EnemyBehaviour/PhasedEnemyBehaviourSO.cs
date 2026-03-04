using UnityEngine;

[CreateAssetMenu(menuName = "Combat/EnemyBehaviour/Phased")]
public class PhasedEnemyBehaviourSO : EnemyBehaviourSO
{
    [SerializeField] private EnemyBehaviourSO phase1;
    [SerializeField] private EnemyBehaviourSO phase2;
    [Range(0f, 1f)][SerializeField] private float phase2AtHpPercent = 0.5f;

    public override EnemyMoveSO PickNextMove(EnemyAIState state, IEnemyActor enemy)
    {
        if (enemy == null) return null;

        // Replace this with CombatContextService.Instance.State if you renamed it
        var combatState = CombatContextService.Instance != null ? CombatContextService.Instance.State : null;

        if (combatState == null || !combatState.TryGet(enemy.Id, out var st))
            return phase1 != null ? phase1.PickNextMove(state, enemy) : null;

        float hpPct = st.MaxHealth <= 0 ? 0 : (st.Health / (float)st.MaxHealth);
        var behaviour = hpPct <= phase2AtHpPercent ? phase2 : phase1;

        return behaviour != null ? behaviour.PickNextMove(state, enemy) : null;
    }
}
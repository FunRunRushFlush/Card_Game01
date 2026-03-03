using UnityEngine;

public class MecanimCombatantAnimator : MonoBehaviour, ICombatantAnimator
{
    [SerializeField] private Animator animator;

    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");

    public void SetIdle() => animator.Play(IdleHash);

    public void Play(CombatantAnim anim)
    {
        switch (anim)
        {
            case CombatantAnim.Idle: animator.Play(IdleHash); break;
            case CombatantAnim.Attack: animator.Play(AttackHash); break;
            case CombatantAnim.Hit: animator.Play(HitHash); break;
            case CombatantAnim.Die: animator.Play(DieHash); break;
        }
    }
}

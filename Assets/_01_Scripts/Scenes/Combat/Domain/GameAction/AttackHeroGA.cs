public class AttackHeroGA : GameAction, IHaveCaster
{
    public EnemyView Attacker { get; private set; }
    public CombatantView Caster { get; private set; }

    // For MultiStrike
    public int? DamageOverride { get; private set; }

    public AttackHeroGA(EnemyView attacker, int? damageOverride = null)
    {
        Attacker = attacker;
        Caster = attacker;
        DamageOverride = damageOverride;
    }
}
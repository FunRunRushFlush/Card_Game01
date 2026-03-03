public class AttackHeroGA : GameAction
{
    public CombatantId AttackerId { get; }
    public CombatantId CasterId { get; }
    public int? DamageOverride { get; } //multistrike

    public AttackHeroGA(CombatantId attackerId, int? damageOverride = null)
    {
        AttackerId = attackerId;
        CasterId = attackerId;
        DamageOverride = damageOverride;
    }
}
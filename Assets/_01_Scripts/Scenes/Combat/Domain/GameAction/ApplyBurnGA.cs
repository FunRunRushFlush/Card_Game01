public class ApplyBurnGA : GameAction
{
    public int BurnDamage { get; }
    public CombatantId Target { get; }

    public ApplyBurnGA(int burnDamage, CombatantId target)
    {
        BurnDamage = burnDamage;
        Target = target;
    }
}
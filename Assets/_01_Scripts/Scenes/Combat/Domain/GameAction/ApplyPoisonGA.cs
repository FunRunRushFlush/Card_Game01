public class ApplyPoisonGA : GameAction
{
    public int PoisonDamage { get; }
    public CombatantId Target { get; }

    public ApplyPoisonGA(int poisonDamage, CombatantId target)
    {
        PoisonDamage = poisonDamage;
        Target = target;
    }
}
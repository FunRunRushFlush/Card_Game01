public class ApplyPoisonGA : GameAction
{
    public CombatantView Target { get; }
    public int PoisonDamage { get; }

    public ApplyPoisonGA(int poisonDamage, CombatantView target)
    {
        PoisonDamage = poisonDamage;
        Target = target;
    }
}

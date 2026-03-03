public class ResolveDeathGA : GameAction
{
    public CombatantId Target { get; }
    public ResolveDeathGA(CombatantId target) => Target = target;
}
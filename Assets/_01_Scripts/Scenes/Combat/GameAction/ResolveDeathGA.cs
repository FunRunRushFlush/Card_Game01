public class ResolveDeathGA : GameAction
{
    public CombatantView Target { get; }

    public ResolveDeathGA(CombatantView target)
    {
        Target = target;
    }
}
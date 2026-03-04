public static class CombatantIds
{

    public static readonly CombatantId Hero = new CombatantId(0);


    public static CombatantId Enemy(int index) => new CombatantId(index + 1);
}
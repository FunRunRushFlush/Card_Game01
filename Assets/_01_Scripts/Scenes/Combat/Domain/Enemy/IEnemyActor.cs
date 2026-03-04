public interface IEnemyActor
{
    CombatantId Id { get; }

    // Base stats (from EnemyData for now)
    int AttackValue { get; }
    int MultiAttackValue { get; }
    int BlockValue { get; }
    int BurnValue { get; }
    int PoisonValue { get; }
    int StrengthValue { get; }
    int WeaknessValue { get; }
}
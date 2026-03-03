public class KillEnemyGA : GameAction
{
    public CombatantId EnemyId { get; }

    public KillEnemyGA(CombatantId enemyId)
    {
        EnemyId = enemyId;
    }
}
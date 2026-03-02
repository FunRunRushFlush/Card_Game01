public enum CardPlayFailCode
{
    None = 0,

    // Core checks
    NoCard,
    SystemNotReady,
    NotEnoughMana,
    NoValidTargets,
    TargetRequired,
    InvalidTarget,
    CombatEnded,
    GamePaused,

    // Conditions
    EnemyCountTooLow,
    MustBeFirstAttackThisTurn,
    RequiresAttackPlayedThisTurn,
    HealthPercentTooHigh,
}

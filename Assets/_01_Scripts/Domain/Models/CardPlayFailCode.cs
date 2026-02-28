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

    // Conditions
    EnemyCountTooLow,
    MustBeFirstAttackThisTurn,
    RequiresAttackPlayedThisTurn,
    HealthPercentTooHigh,
}

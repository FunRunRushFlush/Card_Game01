public enum CardPlayPhase
{
    /// <summary>Used for glow/drag eligibility; target may be null.</summary>
    StartPlay,

    /// <summary>Used when the player tries to actually play the card (drop/release).</summary>
    CommitPlay,
}
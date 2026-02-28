public interface ISession
{
    RunState Run { get; }
    RunDeck Deck { get; }
    RunPerks Perks { get; }
    RunTimer RunTimer { get; }
    RunHeroData Hero { get; }
    CardDatabase CardDatabase { get; }
    RewardSystem RewardSystem { get; }
}

using Game.Scenes.Core;
using UnityEngine;

public class SessionRoot : MonoBehaviour, ISession
{
    [field: SerializeField] public RunState Run { get; private set; }
    [field: SerializeField] public RunDeck Deck { get; private set; }
    [field: SerializeField] public RunPerks Perks { get; private set; }
    [field: SerializeField] public RunTimer RunTimer { get; private set; }
    [field: SerializeField] public RunHeroData Hero { get; private set; }
    [field: SerializeField] public CardDatabase CardDatabase { get; private set; }
    [field: SerializeField] public RewardSystem RewardSystem { get; private set; }



    void Awake()
    {
        CoreManager.Instance.RegisterSession(this);
    }

    void OnDestroy()
    {
        if (CoreManager.Instance != null)
        {
            CoreManager.Instance.ClearSession();
        }
    }
}

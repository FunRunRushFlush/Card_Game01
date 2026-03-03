using System;

public class CombatContextSystem : Singleton<CombatContextSystem>
{
    public static event Action Initialized;

    public CombatHeroState Hero { get; private set; }
    public bool IsInitialized => Hero != null;

    public void Initialize(CombatSetupSnapshot snap)
    {
        Hero = new CombatHeroState(snap);
        Initialized?.Invoke();
    }
}


public static class CombatContextHelpers
{
    public static void SubscribeInitialized(Action callback)
    {
        CombatContextSystem.Initialized += callback;

        // calls imediatly if allready Initialized!
        if (CombatContextSystem.Instance != null && CombatContextSystem.Instance.IsInitialized)
            callback();
    }

    public static void UnsubscribeInitialized(Action callback)
    {
        CombatContextSystem.Initialized -= callback;
    }
}
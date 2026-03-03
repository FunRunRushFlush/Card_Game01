using System;

public class CombatContextService : Singleton<CombatContextService>
{
    public static event Action Initialized;

    public CombatSetupSnapshot Snapshot { get; private set; }
    public CombatHeroState Hero { get; private set; }

    // NEW: single source of truth for combat runtime state
    public CombatState State { get; private set; }

    public bool IsInitialized => Hero != null && State != null;

    public void Initialize(CombatSetupSnapshot snap, CombatState state)
    {
        Snapshot = snap;
        State = state;
        Hero = new CombatHeroState(snap);
        Initialized?.Invoke();
    }

    public void Clear()
    {
        Snapshot = null;
        State = null;
        Hero = null;
    }
}

public static class CombatContextHelpers
{
    public static void SubscribeInitialized(Action callback)
    {
        CombatContextService.Initialized += callback;

        // calls immediately if already initialized
        if (CombatContextService.Instance != null && CombatContextService.Instance.IsInitialized)
            callback();
    }

    public static void UnsubscribeInitialized(Action callback)
    {
        CombatContextService.Initialized -= callback;
    }
}
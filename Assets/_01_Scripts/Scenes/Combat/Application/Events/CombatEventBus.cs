using System;

public static class CombatEventBus
{
    public static event Action<ICombatEvent> OnEvent;

    public static void Publish(ICombatEvent e)
        => OnEvent?.Invoke(e);
}

public interface ICombatEvent { }
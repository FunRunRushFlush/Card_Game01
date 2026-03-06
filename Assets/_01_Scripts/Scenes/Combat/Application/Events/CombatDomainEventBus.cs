using System;

using System;

public static class CombatDomainEventBus
{
    public static event Action<ICombatDomainEvent> OnEvent;

    public static void Publish(ICombatDomainEvent e)
        => OnEvent?.Invoke(e);
}

public interface ICombatDomainEvent { }
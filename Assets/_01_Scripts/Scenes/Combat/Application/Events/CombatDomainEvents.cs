using System;

//CombatDomainEvents.cs
public static class CombatSequence
{
    private static int nextId = 1;
    public static int Next() => nextId++;
}

public sealed class HeroSpawnedEvent : ICombatDomainEvent
{
    public CombatantId HeroId { get; }
    public HeroData Data { get; }

    public HeroSpawnedEvent(CombatantId heroId, HeroData data)
    {
        HeroId = heroId;
        Data = data;
    }
}

public sealed class EnemySpawnedEvent : ICombatDomainEvent
{
    public CombatantId EnemyId { get; }
    public EnemyData Data { get; }

    public EnemySpawnedEvent(CombatantId enemyId, EnemyData data)
    {
        EnemyId = enemyId;
        Data = data;
    }
}

public sealed class EnemyIntentChangedEvent : ICombatDomainEvent
{
    public CombatantId EnemyId { get; }
    public IntentData Intent { get; }

    public EnemyIntentChangedEvent(CombatantId enemyId, IntentData intent)
    {
        EnemyId = enemyId;
        Intent = intent;
    }
}

public sealed class EnemyDiedEvent : ICombatDomainEvent
{
    public CombatantId EnemyId { get; }

    public EnemyDiedEvent(CombatantId enemyId)
    {
        EnemyId = enemyId;
    }
}

public sealed class CombatantStateChangedEvent : ICombatDomainEvent
{
    public CombatantId Id { get; }

    public CombatantStateChangedEvent(CombatantId id)
    {
        Id = id;
    }
}

public sealed class DamageAppliedEvent : ICombatDomainEvent
{
    public CombatantId TargetId { get; }
    public int Amount { get; }
    public CombatantId? SourceId { get; }

    public DamageAppliedEvent(CombatantId targetId, int amount, CombatantId? sourceId)
    {
        TargetId = targetId;
        Amount = amount;
        SourceId = sourceId;
    }
}

public sealed class StatusAddedEvent : ICombatDomainEvent
{
    public CombatantId TargetId { get; }
    public StatusEffectType StatusType { get; }
    public int StackCount { get; }

    public StatusAddedEvent(CombatantId targetId, StatusEffectType statusType, int stackCount)
    {
        TargetId = targetId;
        StatusType = statusType;
        StackCount = stackCount;
    }
}

public sealed class StatusRemovedEvent : ICombatDomainEvent
{
    public CombatantId TargetId { get; }
    public StatusEffectType StatusType { get; }
    public int StackCount { get; }

    public StatusRemovedEvent(CombatantId targetId, StatusEffectType statusType, int stackCount)
    {
        TargetId = targetId;
        StatusType = statusType;
        StackCount = stackCount;
    }
}

public sealed class StatusTickVisualRequestedEvent : ICombatDomainEvent
{
    public CombatantId TargetId { get; }
    public StatusEffectType StatusType { get; }

    public StatusTickVisualRequestedEvent(CombatantId targetId, StatusEffectType statusType)
    {
        TargetId = targetId;
        StatusType = statusType;
    }
}

public sealed class ManaChangedEvent : ICombatDomainEvent
{
    public int CurrentMana { get; }
    public int MaxMana { get; }

    public ManaChangedEvent(int currentMana, int maxMana)
    {
        CurrentMana = currentMana;
        MaxMana = maxMana;
    }
}

public sealed class CardDrawnToHandEvent : ICombatDomainEvent
{
    public Card Card { get; }

    public CardDrawnToHandEvent(Card card)
    {
        Card = card;
    }
}

public sealed class CardDiscardedFromHandEvent : ICombatDomainEvent
{
    public long CardRuntimeId { get; }

    public CardDiscardedFromHandEvent(long cardRuntimeId)
    {
        CardRuntimeId = cardRuntimeId;
    }
}

public sealed class CardPlayedEvent : ICombatDomainEvent
{
    public int SequenceId { get; }
    public CombatantId CasterId { get; }
    public long CardRuntimeId { get; }
    public CombatantId? ManualTargetId { get; }

    public CardPlayedEvent(int sequenceId, CombatantId casterId, long cardRuntimeId, CombatantId? manualTargetId)
    {
        SequenceId = sequenceId;
        CasterId = casterId;
        CardRuntimeId = cardRuntimeId;
        ManualTargetId = manualTargetId;
    }
}

public sealed class AttackDeclaredEvent : ICombatDomainEvent
{
    public int SequenceId { get; }
    public CombatantId AttackerId { get; }
    public CombatantId TargetId { get; }

    public AttackDeclaredEvent(int sequenceId, CombatantId attackerId, CombatantId targetId)
    {
        SequenceId = sequenceId;
        AttackerId = attackerId;
        TargetId = targetId;
    }
}
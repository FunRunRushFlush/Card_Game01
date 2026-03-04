using System;
using System.Collections.Generic;

public sealed class EnemySpawnRequestedEvent : ICombatEvent
{
    public CombatantId EnemyId { get; }
    public EnemyData Data { get; }

    public EnemySpawnRequestedEvent(CombatantId enemyId, EnemyData data)
    {
        EnemyId = enemyId;
        Data = data;
    }
}

public sealed class EnemyIntentChangedEvent : ICombatEvent
{
    public CombatantId EnemyId { get; }
    public IntentData Intent { get; }

    public EnemyIntentChangedEvent(CombatantId enemyId, IntentData intent)
    {
        EnemyId = enemyId;
        Intent = intent;
    }
}

public sealed class EnemyDiedEvent : ICombatEvent
{
    public CombatantId EnemyId { get; }

    public EnemyDiedEvent(CombatantId enemyId)
    {
        EnemyId = enemyId;
    }
}

public sealed class AttackLungeRequestedEvent : ICombatEvent
{
    public CombatantId AttackerId { get; }
    public int Token { get; }

    public AttackLungeRequestedEvent(CombatantId attackerId, int token)
    {
        AttackerId = attackerId;
        Token = token;
    }
}

public sealed class DamageAppliedEvent : ICombatEvent
{
    public CombatantId TargetId { get; }
    public int Amount { get; }
    public CombatantId? Caster { get; }

    public DamageAppliedEvent(CombatantId targetId, int amount, CombatantId? caster)
    {
        TargetId = targetId;
        Amount = amount;
        Caster = caster;
    }
}

public sealed class CombatantStateChangedEvent : ICombatEvent
{
    public CombatantId Id { get; }

    public CombatantStateChangedEvent(CombatantId id)
    {
        Id = id;
    }
}

public sealed class HeroSpawnRequestedEvent : ICombatEvent
{
    public CombatantId HeroId { get; }
    public HeroData Data { get; }

    public HeroSpawnRequestedEvent(CombatantId heroId, HeroData data)
    {
        HeroId = heroId;
        Data = data;
    }
}

public sealed class CardPlayPresentationRequestedEvent : ICombatEvent
{
    public CardView CardView { get; }
    public UnityEngine.Vector3 DiscardPosition { get; }
    public int Token { get; }

    public CardPlayPresentationRequestedEvent(CardView cardView, UnityEngine.Vector3 discardPosition, int token)
    {
        CardView = cardView;
        DiscardPosition = discardPosition;
        Token = token;
    }
}
using System.Collections.Generic;

public sealed class CombatantState
{
    public int MaxHealth { get; private set; }
    public int Health { get; private set; }
    public int Block { get; private set; }

    private readonly Dictionary<StatusEffectType, int> _status = new();

    public CombatantState(int maxHealth)
    {
        MaxHealth = maxHealth;
        Health = maxHealth;
        Block = 0;
    }

    public int GetStatus(StatusEffectType type) => _status.TryGetValue(type, out var v) ? v : 0;

    public void AddStatus(StatusEffectType type, int stacks)
    {
        if (stacks <= 0 || Health <= 0) return;
        _status[type] = GetStatus(type) + stacks;
    }

    public void RemoveStatus(StatusEffectType type, int stacks)
    {
        if (stacks <= 0 || Health <= 0) return;
        if (!_status.ContainsKey(type)) return;

        int next = _status[type] - stacks;
        if (next <= 0) _status.Remove(type);
        else _status[type] = next;
    }

    /// Damage, das zuerst Block frisst (wie eure CombatantView.Damage)
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || Health <= 0) return;

        int remaining = amount;

        if (Block > 0)
        {
            int absorbed = System.Math.Min(Block, remaining);
            Block -= absorbed;
            remaining -= absorbed;
        }

        if (remaining > 0)
        {
            Health -= remaining;
            if (Health < 0) Health = 0;
        }
    }

    public void AddBlock(int amount)
    {
        if (amount <= 0 || Health <= 0) return;
        Block += amount;
    }

    public void ClearBlock() => Block = 0;
}
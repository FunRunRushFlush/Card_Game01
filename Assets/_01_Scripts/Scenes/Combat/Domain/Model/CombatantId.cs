public readonly struct CombatantId
{
    public readonly int Value;
    public CombatantId(int value) => Value = value;
    public override string ToString() => Value.ToString();
}
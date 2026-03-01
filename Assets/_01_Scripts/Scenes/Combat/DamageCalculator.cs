public static class DamageCalculator
{
    public static int Calculate(int baseAmount, int strengthStacks, int weaknessStacks)
    {
        int modified = baseAmount + strengthStacks - weaknessStacks;
        return modified < 0 ? 0 : modified;
    }
}
namespace Genesis.Rules.ValueObjects;

public readonly record struct RulePriority(int Value)
{
    public static readonly RulePriority Lowest = new(0);
    public static readonly RulePriority Low = new(25);
    public static readonly RulePriority Normal = new(50);
    public static readonly RulePriority High = new(75);
    public static readonly RulePriority Highest = new(100);
}

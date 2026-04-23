namespace Genesis.Attention;

public readonly record struct AttentionLevel(double Value)
{
    public static readonly AttentionLevel Zero = new(0.0);
    public static readonly AttentionLevel Low = new(0.25);
    public static readonly AttentionLevel Medium = new(0.5);
    public static readonly AttentionLevel High = new(0.75);
    public static readonly AttentionLevel Critical = new(1.0);

    public bool ShouldCollapse => Value >= High.Value;
}

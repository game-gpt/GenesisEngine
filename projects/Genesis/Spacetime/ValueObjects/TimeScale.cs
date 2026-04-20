namespace Genesis.Spacetime.ValueObjects;

public readonly record struct TimeScale(double Value)
{
    public static readonly TimeScale L0 = new(1.0);
    public static readonly TimeScale L1 = new(60.0);
    public static readonly TimeScale L2 = new(3600.0);
    public static readonly TimeScale L3 = new(86400.0);
}

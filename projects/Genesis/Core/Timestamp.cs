namespace Genesis.Core;

public readonly record struct Timestamp(DateTime Value)
{
    public long UnixTimeSeconds => ((DateTimeOffset)Value).ToUnixTimeSeconds();
    public long UnixTimeMilliseconds => ((DateTimeOffset)Value).ToUnixTimeMilliseconds();

    public static Timestamp Now => new(DateTime.UtcNow);
    public static Timestamp FromUnixTimeSeconds(long seconds) => new(DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime);
    public static Timestamp FromUnixTimeMilliseconds(long milliseconds) => new(DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime);
}

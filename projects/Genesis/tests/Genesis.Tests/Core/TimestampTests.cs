using Genesis.Core;
using Xunit;

namespace Genesis.Tests.Core;

public class TimestampTests
{
    [Fact]
    public void Now_ReturnsCurrentUtcTime()
    {
        var before = DateTime.UtcNow;
        var timestamp = Timestamp.Now;
        var after = DateTime.UtcNow;

        Assert.True(timestamp.Value >= before);
        Assert.True(timestamp.Value <= after);
    }

    [Fact]
    public void FromUnixTimeSeconds_RoundTrip_PreservesValue()
    {
        var seconds = 1700000000L;
        var timestamp = Timestamp.FromUnixTimeSeconds(seconds);

        Assert.Equal(seconds, timestamp.UnixTimeSeconds);
    }

    [Fact]
    public void FromUnixTimeMilliseconds_RoundTrip_PreservesValue()
    {
        var milliseconds = 1700000000000L;
        var timestamp = Timestamp.FromUnixTimeMilliseconds(milliseconds);

        Assert.Equal(milliseconds, timestamp.UnixTimeMilliseconds);
    }

    [Fact]
    public void UnixTimeSeconds_ReturnsCorrectValue()
    {
        var dateTime = new DateTime(2023, 11, 14, 22, 13, 20, DateTimeKind.Utc);
        var timestamp = new Timestamp(dateTime);

        var expected = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        Assert.Equal(expected, timestamp.UnixTimeSeconds);
    }
}

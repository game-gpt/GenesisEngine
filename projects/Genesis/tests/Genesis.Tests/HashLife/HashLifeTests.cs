using Genesis.Core;
using Genesis.HashLife;
using Xunit;

namespace Genesis.Tests.HashLife;

public class CacheEntryTests
{
    [Fact]
    public void IsExpired_WithinMaxAge_ReturnsFalse()
    {
        var entry = new CacheEntry(1, 2, 1.0f, Timestamp.Now);
        var maxAge = TimeSpan.FromHours(1);

        Assert.False(entry.IsExpired(maxAge));
    }

    [Fact]
    public void IsExpired_ExceedsMaxAge_ReturnsTrue()
    {
        var oldTime = DateTime.UtcNow.AddHours(-2);
        var entry = new CacheEntry(1, 2, 1.0f, new Timestamp(oldTime));
        var maxAge = TimeSpan.FromHours(1);

        Assert.True(entry.IsExpired(maxAge));
    }

    [Fact]
    public void IsExpired_ExactlyAtMaxAge_ReturnsTrue()
    {
        var oldTime = DateTime.UtcNow.AddHours(-1);
        var entry = new CacheEntry(1, 2, 1.0f, new Timestamp(oldTime));
        var maxAge = TimeSpan.FromHours(1);

        Assert.True(entry.IsExpired(maxAge));
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var time = new Timestamp(DateTime.UtcNow);
        var e1 = new CacheEntry(1, 2, 1.0f, time);
        var e2 = new CacheEntry(1, 2, 1.0f, time);

        Assert.Equal(e1, e2);
    }
}

public class EvolutionKeyTests
{
    [Fact]
    public void Create_ReturnsCorrectKey()
    {
        var key = EvolutionKey.Create(42, 1.0f);

        Assert.Equal(42UL, key.NodeHash);
        Assert.Equal(1.0f, key.DeltaTime);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var k1 = new EvolutionKey(42, 1.0f);
        var k2 = new EvolutionKey(42, 1.0f);

        Assert.Equal(k1, k2);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var k1 = new EvolutionKey(42, 1.0f);
        var k2 = new EvolutionKey(43, 1.0f);

        Assert.NotEqual(k1, k2);
    }

    [Fact]
    public void RecordEquality_DifferentDeltaTime_AreNotEqual()
    {
        var k1 = new EvolutionKey(42, 1.0f);
        var k2 = new EvolutionKey(42, 2.0f);

        Assert.NotEqual(k1, k2);
    }
}

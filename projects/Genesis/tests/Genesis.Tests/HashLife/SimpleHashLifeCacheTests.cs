using Genesis.HashLife;
using Xunit;

namespace Genesis.Tests.HashLife;

public class SimpleHashLifeCacheTests
{
    #region TryGet 测试

    [Fact]
    public void TryGet_EmptyCache_ReturnsFalse()
    {
        var cache = new SimpleHashLifeCache();

        var result = cache.TryGet(42, 1.0f, out var resultHash);

        Assert.False(result);
        Assert.Equal(0UL, resultHash);
    }

    [Fact]
    public void TryGet_AfterSet_ReturnsTrue()
    {
        var cache = new SimpleHashLifeCache();
        cache.Set(42, 1.0f, 100);

        var result = cache.TryGet(42, 1.0f, out var resultHash);

        Assert.True(result);
        Assert.Equal(100UL, resultHash);
    }

    [Fact]
    public void TryGet_DifferentDeltaTime_ReturnsFalse()
    {
        var cache = new SimpleHashLifeCache();
        cache.Set(42, 1.0f, 100);

        var result = cache.TryGet(42, 2.0f, out var resultHash);

        Assert.False(result);
        Assert.Equal(0UL, resultHash);
    }

    [Fact]
    public void TryGet_DifferentNodeHash_ReturnsFalse()
    {
        var cache = new SimpleHashLifeCache();
        cache.Set(42, 1.0f, 100);

        var result = cache.TryGet(43, 1.0f, out var resultHash);

        Assert.False(result);
        Assert.Equal(0UL, resultHash);
    }

    #endregion

    #region Set 测试

    [Fact]
    public void Set_OverwritesExistingEntry()
    {
        var cache = new SimpleHashLifeCache();
        cache.Set(42, 1.0f, 100);
        cache.Set(42, 1.0f, 200);

        cache.TryGet(42, 1.0f, out var resultHash);

        Assert.Equal(200UL, resultHash);
    }

    [Fact]
    public void Set_IncrementsCount()
    {
        var cache = new SimpleHashLifeCache();

        cache.Set(1, 1.0f, 10);
        cache.Set(2, 1.0f, 20);

        Assert.Equal(2, cache.Count);
    }

    #endregion

    #region Invalidate 测试

    [Fact]
    public void Invalidate_RemovesEntriesWithSameNodeHash()
    {
        var cache = new SimpleHashLifeCache();
        cache.Set(42, 1.0f, 100);
        cache.Set(42, 2.0f, 200);
        cache.Set(43, 1.0f, 300);

        cache.Invalidate(42);

        Assert.False(cache.TryGet(42, 1.0f, out _));
        Assert.False(cache.TryGet(42, 2.0f, out _));
        Assert.True(cache.TryGet(43, 1.0f, out _));
    }

    [Fact]
    public void Invalidate_NonExistingNodeHash_DoesNothing()
    {
        var cache = new SimpleHashLifeCache();
        cache.Set(42, 1.0f, 100);

        cache.Invalidate(99);

        Assert.Equal(1, cache.Count);
    }

    #endregion

    #region Clear 测试

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var cache = new SimpleHashLifeCache();
        cache.Set(1, 1.0f, 10);
        cache.Set(2, 1.0f, 20);
        cache.Set(3, 1.0f, 30);

        cache.Clear();

        Assert.Equal(0, cache.Count);
    }

    #endregion

    #region 缓存命中/失效场景

    [Fact]
    public void CacheHitMissScenario_SetGetInvalidateGet()
    {
        var cache = new SimpleHashLifeCache();

        cache.Set(1, 1.0f, 100);
        Assert.True(cache.TryGet(1, 1.0f, out var hash1));
        Assert.Equal(100UL, hash1);

        cache.Invalidate(1);
        Assert.False(cache.TryGet(1, 1.0f, out _));

        cache.Set(1, 1.0f, 200);
        Assert.True(cache.TryGet(1, 1.0f, out var hash2));
        Assert.Equal(200UL, hash2);
    }

    [Fact]
    public void MultipleDeltaTimes_SameNodeHash()
    {
        var cache = new SimpleHashLifeCache();

        cache.Set(42, 1.0f, 100);
        cache.Set(42, 2.0f, 200);
        cache.Set(42, 4.0f, 400);

        Assert.True(cache.TryGet(42, 1.0f, out var h1));
        Assert.True(cache.TryGet(42, 2.0f, out var h2));
        Assert.True(cache.TryGet(42, 4.0f, out var h4));

        Assert.Equal(100UL, h1);
        Assert.Equal(200UL, h2);
        Assert.Equal(400UL, h4);
    }

    [Fact]
    public void SizeBytes_IncreasesWithEntries()
    {
        var cache = new SimpleHashLifeCache();

        var initialSize = cache.SizeBytes;
        cache.Set(1, 1.0f, 10);
        cache.Set(2, 1.0f, 20);

        Assert.True(cache.SizeBytes > initialSize);
        Assert.Equal(2, cache.Count);
    }

    #endregion
}

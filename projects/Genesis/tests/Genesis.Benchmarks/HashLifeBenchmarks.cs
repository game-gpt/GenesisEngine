using BenchmarkDotNet.Attributes;
using Genesis.HashLife;

namespace Genesis.Benchmarks;

[MemoryDiagnoser]
public class HashLifeBenchmarks
{
    private SimpleHashLifeCache _cache = null!;
    private const int EntryCount = 10000;

    [GlobalSetup]
    public void Setup()
    {
        _cache = new SimpleHashLifeCache(TimeSpan.FromMinutes(10));

        for (ulong i = 0; i < (ulong)EntryCount; i++)
        {
            _cache.Set(i, 1.0f, i * 2);
        }
    }

    [Benchmark(Description = "HashLife 缓存写入")]
    public void Set()
    {
        var cache = new SimpleHashLifeCache(TimeSpan.FromMinutes(10));
        for (ulong i = 0; i < (ulong)EntryCount; i++)
        {
            cache.Set(i + 100000, 1.0f, i * 3);
        }
    }

    [Benchmark(Description = "HashLife 缓存命中")]
    public int TryGet_Hit()
    {
        var hits = 0;
        for (ulong i = 0; i < (ulong)EntryCount; i++)
        {
            if (_cache.TryGet(i, 1.0f, out _))
            {
                hits++;
            }
        }
        return hits;
    }

    [Benchmark(Description = "HashLife 缓存未命中")]
    public int TryGet_Miss()
    {
        var misses = 0;
        for (ulong i = 0; i < (ulong)EntryCount; i++)
        {
            if (!_cache.TryGet(i + 999999, 1.0f, out _))
            {
                misses++;
            }
        }
        return misses;
    }

    [Benchmark(Description = "HashLife 缓存失效")]
    public void Invalidate()
    {
        var cache = new SimpleHashLifeCache(TimeSpan.FromMinutes(10));
        for (ulong i = 0; i < (ulong)EntryCount; i++)
        {
            cache.Set(i + 200000, 1.0f, i * 4);
        }
        for (ulong i = 0; i < (ulong)EntryCount; i++)
        {
            cache.Invalidate(i + 200000);
        }
    }
}

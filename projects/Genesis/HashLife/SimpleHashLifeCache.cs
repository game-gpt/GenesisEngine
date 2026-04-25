using Genesis.Core;

namespace Genesis.HashLife;

public sealed class SimpleHashLifeCache : IHashLifeCache
{
    #region 字段

    private readonly Dictionary<EvolutionKey, CacheEntry> _cache;
    private readonly TimeSpan _maxAge;

    #endregion

    #region 构造函数

    public SimpleHashLifeCache(TimeSpan? maxAge = null)
    {
        _cache = new Dictionary<EvolutionKey, CacheEntry>();
        _maxAge = maxAge ?? TimeSpan.FromMinutes(10);
    }

    #endregion

    #region IHashLifeCache 实现

    public int Count => _cache.Count;

    public long SizeBytes => _cache.Count * (sizeof(ulong) * 2 + sizeof(float) + 16);

    public bool TryGet(ulong nodeHash, float deltaTime, out ulong resultHash)
    {
        var key = EvolutionKey.Create(nodeHash, deltaTime);

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired(_maxAge))
            {
                _cache.Remove(key);
                resultHash = 0;
                return false;
            }

            resultHash = entry.ResultHash;
            return true;
        }

        resultHash = 0;
        return false;
    }

    public void Set(ulong nodeHash, float deltaTime, ulong resultHash)
    {
        var key = EvolutionKey.Create(nodeHash, deltaTime);
        var entry = new CacheEntry(nodeHash, resultHash, deltaTime, Timestamp.Now);
        _cache[key] = entry;
    }

    public void Invalidate(ulong nodeHash)
    {
        var keysToRemove = _cache.Keys
            .Where(k => k.NodeHash == nodeHash)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
    }

    public void Clear()
    {
        _cache.Clear();
    }

    #endregion
}

using Genesis.Core;

namespace Genesis.HashLife;

public readonly record struct CacheEntry(
    ulong NodeHash,
    ulong ResultHash,
    float DeltaTime,
    Timestamp CreatedAt)
{
    public bool IsExpired(TimeSpan maxAge) => DateTime.UtcNow - CreatedAt.Value > maxAge;
}

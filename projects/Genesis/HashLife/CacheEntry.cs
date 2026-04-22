using Genesis.Core.ValueObjects;

namespace Genesis.HashLife.ValueObjects;

public readonly record struct CacheEntry(
    ulong NodeHash,
    ulong ResultHash,
    float DeltaTime,
    Timestamp CreatedAt)
{
    public bool IsExpired(TimeSpan maxAge) => DateTime.UtcNow - CreatedAt.Value > maxAge;
}

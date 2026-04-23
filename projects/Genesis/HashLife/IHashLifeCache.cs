namespace Genesis.HashLife;

public interface IHashLifeCache
{
    bool TryGet(ulong nodeHash, float deltaTime, out ulong resultHash);
    void Set(ulong nodeHash, float deltaTime, ulong resultHash);
    void Invalidate(ulong nodeHash);
    void Clear();
    int Count { get; }
    long SizeBytes { get; }
}

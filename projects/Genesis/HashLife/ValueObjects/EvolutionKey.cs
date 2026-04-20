namespace Genesis.HashLife.ValueObjects;

public readonly record struct EvolutionKey(ulong NodeHash, float DeltaTime)
{
    public static EvolutionKey Create(ulong nodeHash, float deltaTime) => new(nodeHash, deltaTime);
}

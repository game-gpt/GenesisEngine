namespace Genesis.World.ValueObjects;

public readonly record struct WorldSeed(ulong Value)
{
    public static WorldSeed New() => new((ulong)Random.Shared.NextInt64());
    public static WorldSeed FromString(string input) => new(HashString(input));

    private static ulong HashString(string input)
    {
        ulong hash = 5381;
        foreach (var c in input)
        {
            hash = ((hash << 5) + hash) ^ c;
        }
        return hash;
    }
}

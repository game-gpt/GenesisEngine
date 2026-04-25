using Genesis.World;
using Xunit;

namespace Genesis.Tests.World;

public class WorldSeedTests
{
    [Fact]
    public void New_ReturnsNonZeroSeed()
    {
        var seed = WorldSeed.New();

        Assert.NotEqual(0UL, seed.Value);
    }

    [Fact]
    public void New_ReturnsDifferentSeeds()
    {
        var seeds = new HashSet<ulong>();
        for (var i = 0; i < 100; i++)
        {
            seeds.Add(WorldSeed.New().Value);
        }

        Assert.True(seeds.Count > 90);
    }

    [Fact]
    public void FromString_SameInput_SameOutput()
    {
        var s1 = WorldSeed.FromString("test-world");
        var s2 = WorldSeed.FromString("test-world");

        Assert.Equal(s1, s2);
    }

    [Fact]
    public void FromString_DifferentInput_DifferentOutput()
    {
        var s1 = WorldSeed.FromString("world-a");
        var s2 = WorldSeed.FromString("world-b");

        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void FromString_EmptyString_ReturnsNonZero()
    {
        var seed = WorldSeed.FromString("");

        Assert.NotEqual(0UL, seed.Value);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var s1 = new WorldSeed(42);
        var s2 = new WorldSeed(42);

        Assert.Equal(s1, s2);
    }
}

using Genesis.Core;
using Genesis.World;
using Xunit;

namespace Genesis.Tests.World;

public class SimpleNoiseGeneratorTests
{
    [Fact]
    public void Noise2D_ReturnsValueInRange()
    {
        var gen = new SimpleNoiseGenerator(42);

        for (var i = 0; i < 100; i++)
        {
            var x = Random.Shared.NextDouble() * 100;
            var y = Random.Shared.NextDouble() * 100;
            var value = gen.Noise2D(x, y);
            Assert.InRange(value, -1.0, 1.0);
        }
    }

    [Fact]
    public void Noise3D_ReturnsValueInRange()
    {
        var gen = new SimpleNoiseGenerator(42);

        for (var i = 0; i < 100; i++)
        {
            var x = Random.Shared.NextDouble() * 100;
            var y = Random.Shared.NextDouble() * 100;
            var z = Random.Shared.NextDouble() * 100;
            var value = gen.Noise3D(x, y, z);
            Assert.InRange(value, -1.0, 1.0);
        }
    }

    [Fact]
    public void Noise2D_SameInput_SameOutput()
    {
        var gen = new SimpleNoiseGenerator(42);
        var v1 = gen.Noise2D(10.5, 20.3);
        var v2 = gen.Noise2D(10.5, 20.3);
        Assert.Equal(v1, v2);
    }

    [Fact]
    public void Noise2D_DifferentSeeds_DifferentOutput()
    {
        var gen1 = new SimpleNoiseGenerator(42);
        var gen2 = new SimpleNoiseGenerator(99);
        var v1 = gen1.Noise2D(10.5, 20.3);
        var v2 = gen2.Noise2D(10.5, 20.3);
        Assert.NotEqual(v1, v2);
    }

    [Fact]
    public void GenerateNoiseMap_ReturnsCorrectSize()
    {
        var gen = new SimpleNoiseGenerator(42);
        var bounds = new Bounds(0, 0, 0, 100, 100, 0);
        var map = gen.GenerateNoiseMap(bounds, 64, 42);
        Assert.Equal(64 * 64, map.Length);
    }

    [Fact]
    public void SetOctaves_ChangesOutput()
    {
        var gen = new SimpleNoiseGenerator(42);
        gen.SetOctaves(1);
        var v1 = gen.Noise2D(10.5, 20.3);
        gen.SetOctaves(8);
        var v2 = gen.Noise2D(10.5, 20.3);
        Assert.NotEqual(v1, v2);
    }
}

using Genesis.Collapse;
using Genesis.Core;
using Genesis.World;
using Xunit;

namespace Genesis.Tests.World;

public class SimpleWFCGeneratorTests
{
    [Fact]
    public void GenerateTilemap_ReturnsNonZeroHash()
    {
        var gen = new SimpleWFCGenerator();
        var regionId = RegionId.New();
        var hash = gen.GenerateTilemap(regionId, 8, 8, 42);
        Assert.NotEqual(0UL, hash);
    }

    [Fact]
    public void ValidateTilemap_ExistingHash_ReturnsTrue()
    {
        var gen = new SimpleWFCGenerator();
        var regionId = RegionId.New();
        var hash = gen.GenerateTilemap(regionId, 8, 8, 42);
        Assert.True(gen.ValidateTilemap(hash));
    }

    [Fact]
    public void ValidateTilemap_NonexistentHash_ReturnsFalse()
    {
        var gen = new SimpleWFCGenerator();
        Assert.False(gen.ValidateTilemap(12345UL));
    }

    [Fact]
    public void AddConstraint_DoesNotThrow()
    {
        var gen = new SimpleWFCGenerator();
        gen.AddConstraint("test", 1);
    }

    [Fact]
    public void RemoveConstraint_DoesNotThrow()
    {
        var gen = new SimpleWFCGenerator();
        gen.AddConstraint("test", 1);
        gen.RemoveConstraint("test");
    }
}

public class SimpleWorldGeneratorTests
{
    [Fact]
    public void GenerateRegion_ReturnsNonZeroHash()
    {
        var noise = new SimpleNoiseGenerator(42);
        var wfc = new SimpleWFCGenerator();
        var gen = new SimpleWorldGenerator(noise, wfc);
        var regionId = RegionId.New();
        var hash = gen.GenerateRegion(regionId, 42);
        Assert.NotEqual(0UL, hash);
    }

    [Fact]
    public void ValidateRegion_CorrectHash_ReturnsTrue()
    {
        var noise = new SimpleNoiseGenerator(42);
        var wfc = new SimpleWFCGenerator();
        var gen = new SimpleWorldGenerator(noise, wfc);
        var regionId = RegionId.New();
        var hash = gen.GenerateRegion(regionId, 42);
        Assert.True(gen.ValidateRegion(regionId, hash));
    }

    [Fact]
    public void DetermineBiome_OceanElevation_ReturnsOcean()
    {
        var noise = new SimpleNoiseGenerator(42);
        var wfc = new SimpleWFCGenerator();
        var gen = new SimpleWorldGenerator(noise, wfc);
        var biome = gen.DetermineBiome(0.5, 0.5, 0.1);
        Assert.Equal(BiomeType.Ocean, biome);
    }

    [Fact]
    public void DetermineBiome_HighElevation_ReturnsMountains()
    {
        var noise = new SimpleNoiseGenerator(42);
        var wfc = new SimpleWFCGenerator();
        var gen = new SimpleWorldGenerator(noise, wfc);
        var biome = gen.DetermineBiome(0.5, 0.5, 0.9);
        Assert.Equal(BiomeType.Mountains, biome);
    }

    [Fact]
    public void DetermineBiome_ColdTemp_ReturnsTundra()
    {
        var noise = new SimpleNoiseGenerator(42);
        var wfc = new SimpleWFCGenerator();
        var gen = new SimpleWorldGenerator(noise, wfc);
        var biome = gen.DetermineBiome(0.1, 0.5, 0.5);
        Assert.Equal(BiomeType.Tundra, biome);
    }

    [Fact]
    public void SetBiome_GetBiome_ReturnsSetBiome()
    {
        var noise = new SimpleNoiseGenerator(42);
        var wfc = new SimpleWFCGenerator();
        var gen = new SimpleWorldGenerator(noise, wfc);
        var regionId = RegionId.New();
        gen.SetBiome(regionId, BiomeType.Volcanic);
        Assert.Equal(BiomeType.Volcanic, gen.GetBiome(regionId));
    }

    [Fact]
    public void GetElevation_ReturnsValueInRange()
    {
        var noise = new SimpleNoiseGenerator(42);
        var wfc = new SimpleWFCGenerator();
        var gen = new SimpleWorldGenerator(noise, wfc);
        var elevation = gen.GetElevation(100, 200, 42);
        Assert.True(elevation >= 0);
    }
}

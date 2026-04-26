using Genesis.Core;

namespace Genesis.World;

public sealed class SimpleWorldGenerator : IWorldGenerator
{
    #region 字段

    private readonly INoiseGenerator _noiseGenerator;
    private readonly IWFCGenerator _wfcGenerator;
    private readonly Dictionary<RegionId, BiomeType> _biomeMap;
    private readonly Dictionary<ulong, ulong> _regionCache;

    #endregion

    #region 构造函数

    public SimpleWorldGenerator(INoiseGenerator noiseGenerator, IWFCGenerator wfcGenerator)
    {
        _noiseGenerator = noiseGenerator;
        _wfcGenerator = wfcGenerator;
        _biomeMap = new Dictionary<RegionId, BiomeType>();
        _regionCache = new Dictionary<ulong, ulong>();
    }

    #endregion

    #region IWorldGenerator 实现

    public ulong GenerateRegion(RegionId regionId, ulong seed)
    {
        var biome = GetBiome(regionId);
        var noiseValue = _noiseGenerator.Noise2D(
            regionId.Value.GetHashCode() * 0.01,
            seed * 0.01);

        var regionHash = ComputeRegionHash(regionId, biome, noiseValue);
        _regionCache[(ulong)regionId.Value.GetHashCode()] = regionHash;

        return regionHash;
    }

    public bool ValidateRegion(RegionId regionId, ulong regionHash)
    {
        return _regionCache.TryGetValue((ulong)regionId.Value.GetHashCode(), out var cached) &&
               cached == regionHash;
    }

    public void SetBiome(RegionId regionId, BiomeType biome)
    {
        _biomeMap[regionId] = biome;
    }

    public BiomeType GetBiome(RegionId regionId)
    {
        return _biomeMap.TryGetValue(regionId, out var biome) ? biome : BiomeType.Plains;
    }

    #endregion

    #region 公开方法

    public BiomeType DetermineBiome(double temperature, double moisture, double elevation)
    {
        if (elevation < 0.2) return BiomeType.Ocean;
        if (elevation > 0.8) return BiomeType.Mountains;

        if (temperature < 0.2) return BiomeType.Tundra;
        if (temperature > 0.8 && moisture < 0.3) return BiomeType.Desert;
        if (temperature > 0.7 && moisture > 0.6) return BiomeType.Jungle;
        if (moisture > 0.7) return BiomeType.Swamp;
        if (temperature > 0.6 && elevation > 0.5) return BiomeType.Volcanic;

        if (moisture > 0.4 && temperature > 0.3) return BiomeType.Forest;
        if (elevation > 0.5) return BiomeType.Crystal;

        return BiomeType.Plains;
    }

    public double GetElevation(int worldX, int worldZ, ulong seed)
    {
        var nx = worldX * 0.005;
        var nz = worldZ * 0.005;

        var elevation = _noiseGenerator.Noise2D(nx + seed * 0.001, nz);
        elevation = Math.Pow(elevation * 0.5 + 0.5, 1.5);

        return elevation;
    }

    public double GetTemperature(int worldX, int worldZ, ulong seed)
    {
        var nx = worldX * 0.003;
        var nz = worldZ * 0.003;

        return _noiseGenerator.Noise2D(nx + seed * 0.002 + 1000, nz) * 0.5 + 0.5;
    }

    public double GetMoisture(int worldX, int worldZ, ulong seed)
    {
        var nx = worldX * 0.004;
        var nz = worldZ * 0.004;

        return _noiseGenerator.Noise2D(nx + seed * 0.003 + 2000, nz) * 0.5 + 0.5;
    }

    #endregion

    #region 私有方法

    private static ulong ComputeRegionHash(RegionId regionId, BiomeType biome, double noiseValue)
    {
        ulong hash = 14695981039346656037;
        var idBytes = regionId.Value.ToByteArray();

        foreach (var b in idBytes)
        {
            hash ^= b;
            hash *= 1099511628211;
        }

        hash ^= (ulong)biome;
        hash *= 1099511628211;

        var noiseBits = BitConverter.DoubleToUInt64Bits(noiseValue);
        hash ^= noiseBits;
        hash *= 1099511628211;

        return hash;
    }

    #endregion
}

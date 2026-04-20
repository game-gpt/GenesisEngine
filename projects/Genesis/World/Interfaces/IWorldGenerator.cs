using Genesis.Core.ValueObjects;

namespace Genesis.World.Interfaces;

public interface IWorldGenerator
{
    ulong GenerateRegion(RegionId regionId, ulong seed);
    bool ValidateRegion(RegionId regionId, ulong regionHash);
    void SetBiome(RegionId regionId, BiomeType biome);
    BiomeType GetBiome(RegionId regionId);
}

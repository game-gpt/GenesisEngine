using Genesis.Core;

namespace Genesis.World;

public interface IWFCGenerator
{
    ulong GenerateTilemap(RegionId regionId, int width, int height, ulong seed);
    bool ValidateTilemap(ulong tilemapHash);
    void AddConstraint(string name, int priority);
    void RemoveConstraint(string name);
}

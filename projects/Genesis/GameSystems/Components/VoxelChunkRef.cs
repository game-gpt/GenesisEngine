namespace Genesis.GameSystems.Components;

public readonly record struct VoxelChunkRef(
    string ChunkName,
    int SizeX,
    int SizeY,
    int SizeZ,
    float OffsetX = 0f,
    float OffsetY = 0f,
    float OffsetZ = 0f)
{
    public static VoxelChunkRef Empty => new(string.Empty, 0, 0, 0);
    public bool IsValid => !string.IsNullOrEmpty(ChunkName) && SizeX > 0 && SizeY > 0 && SizeZ > 0;
}

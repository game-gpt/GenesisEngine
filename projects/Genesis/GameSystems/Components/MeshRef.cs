namespace Genesis.GameSystems.Components;

public readonly record struct MeshRef(string AssetPath, bool IsVoxel = false)
{
    public static MeshRef Empty => new(string.Empty);
    public bool IsValid => !string.IsNullOrEmpty(AssetPath);
}

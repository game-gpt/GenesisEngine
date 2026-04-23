using Genesis.Core;

namespace Genesis.Rendering;

public readonly record struct SceneDescription(
    ulong SceneHash,
    Timestamp GeneratedAt,
    int EntityCount,
    Bounds Bounds)
{
    public bool IsValid => EntityCount >= 0 && SceneHash != 0;
}

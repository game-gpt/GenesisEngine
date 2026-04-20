using Genesis.Core.ValueObjects;

namespace Genesis.Rendering.ValueObjects;

public readonly record struct SceneDescription(
    ulong SceneHash,
    Timestamp GeneratedAt,
    int EntityCount,
    Bounds Bounds)
{
    public bool IsValid => EntityCount >= 0 && SceneHash != 0;
}

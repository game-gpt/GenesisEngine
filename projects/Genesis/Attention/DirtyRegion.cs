using Genesis.Core;

namespace Genesis.Attention;

public readonly record struct DirtyRegion(
    ulong RegionId,
    Bounds Bounds,
    NodeLevel Level,
    DateTime LastModified,
    bool IsDirty)
{
    public DirtyRegion MarkDirty() => this with { IsDirty = true, LastModified = DateTime.UtcNow };
    public DirtyRegion MarkClean() => this with { IsDirty = false };
}

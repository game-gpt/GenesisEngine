using Genesis.Core.Enums;
using Genesis.Core.ValueObjects;

namespace Genesis.Attention.ValueObjects;

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

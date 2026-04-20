using Genesis.Core.Enums;
using Genesis.Core.ValueObjects;

namespace Genesis.Attention.Interfaces;

public interface IDirtyRegion
{
    ulong RegionId { get; }
    Bounds Bounds { get; }
    NodeLevel Level { get; }
    DateTime LastModified { get; }
    bool IsDirty { get; }
    void MarkDirty();
    void MarkClean();
}

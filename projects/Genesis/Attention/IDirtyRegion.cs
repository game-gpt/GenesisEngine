using Genesis.Core;

namespace Genesis.Attention;

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

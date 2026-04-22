using Genesis.Core.Enums;
using Genesis.Core.ValueObjects;

namespace Genesis.Spacetime.Interfaces;

public interface ISpacetimeNode
{
    ulong SpatialHash { get; }
    ulong HistoryHash { get; }
    NodeLevel Level { get; }
    Bounds Bounds { get; }
    double TimeScale { get; }
    CollapseState CollapseState { get; }
    IReadOnlyList<ISpacetimeNode> Children { get; }
    void UpdateHistoryHash();
    void InvalidateCache();
}

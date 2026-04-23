using Genesis.Core;

namespace Genesis.Spacetime;

public readonly record struct SpacetimeNode(
    ulong SpatialHash,
    ulong HistoryHash,
    NodeLevel Level,
    Bounds Bounds,
    double TimeScale,
    CollapseState CollapseState)
{
    public double NormalizedTimeScale => TimeScale / (1 << (int)Level);
}

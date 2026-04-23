using Genesis.Core;

namespace Genesis.Spacetime;

public interface ISpacetimeTree
{
    ISpacetimeNode? Root { get; }
    ISpacetimeNode? GetNode(ulong spatialHash);
    ISpacetimeNode? FindNode(Position position, NodeLevel level);
    void InsertNode(ISpacetimeNode node);
    void RemoveNode(ulong spatialHash);
    void UpdateHistoryHash(ulong spatialHash);
}

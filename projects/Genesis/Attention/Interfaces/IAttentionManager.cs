using Genesis.Core.Enums;
using Genesis.Core.ValueObjects;
using Genesis.Spacetime.Interfaces;
using Genesis.Attention.ValueObjects;

namespace Genesis.Attention.Interfaces;

public interface IAttentionManager
{
    double CalculateAttention(Position position, Position playerPosition, Position playerViewDirection);
    IReadOnlyList<ISpacetimeNode> GetHighAttentionNodes();
    IReadOnlyList<ISpacetimeNode> GetNodesInViewport();
    void UpdateAttention(Position playerPosition, Position playerViewDirection);
    void AddInterestPoint(InterestPoint point);
    void RemoveInterestPoint(ulong id);
}

using Genesis.Core;
using Genesis.Spacetime;

namespace Genesis.Attention;

public interface IAttentionManager
{
    double CalculateAttention(Position position, Position playerPosition, Position playerViewDirection);
    IReadOnlyList<ISpacetimeNode> GetHighAttentionNodes();
    IReadOnlyList<ISpacetimeNode> GetNodesInViewport();
    void UpdateAttention(Position playerPosition, Position playerViewDirection);
    void AddInterestPoint(InterestPoint point);
    void RemoveInterestPoint(ulong id);
}

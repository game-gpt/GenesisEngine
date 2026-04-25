using Genesis.Core;
using Genesis.Spacetime;

namespace Genesis.Attention;

public sealed class SimpleAttentionManager : IAttentionManager
{
    #region 字段

    private readonly List<InterestPoint> _interestPoints;
    private readonly ISpacetimeTree _spacetimeTree;
    private readonly IAttentionModel _attentionModel;
    private Position _playerPosition;
    private Position _playerViewDirection;

    #endregion

    #region 构造函数

    public SimpleAttentionManager(ISpacetimeTree spacetimeTree, IAttentionModel attentionModel)
    {
        _interestPoints = new List<InterestPoint>();
        _spacetimeTree = spacetimeTree;
        _attentionModel = attentionModel;
        _playerPosition = Position.Zero;
        _playerViewDirection = new Position(0, 0, 1);
    }

    #endregion

    #region IAttentionManager 实现

    public double CalculateAttention(Position position, Position playerPosition, Position playerViewDirection)
    {
        var distance = _attentionModel.CalculateDistanceFactor(position, playerPosition);
        var view = _attentionModel.CalculateViewFactor(position, playerPosition, playerViewDirection);
        var interaction = _attentionModel.CalculateInteractionFactor(0);
        var causal = _attentionModel.CalculateCausalImportance(0);

        return _attentionModel.CombineFactors(distance, view, interaction, causal);
    }

    public IReadOnlyList<ISpacetimeNode> GetHighAttentionNodes()
    {
        var result = new List<ISpacetimeNode>();

        if (_spacetimeTree.Root is null)
        {
            return result.AsReadOnly();
        }

        CollectHighAttentionNodes(_spacetimeTree.Root, result);
        return result.AsReadOnly();
    }

    public IReadOnlyList<ISpacetimeNode> GetNodesInViewport()
    {
        var result = new List<ISpacetimeNode>();

        if (_spacetimeTree.Root is null)
        {
            return result.AsReadOnly();
        }

        CollectNodesInViewport(_spacetimeTree.Root, result);
        return result.AsReadOnly();
    }

    public void UpdateAttention(Position playerPosition, Position playerViewDirection)
    {
        _playerPosition = playerPosition;
        _playerViewDirection = playerViewDirection;
    }

    public void AddInterestPoint(InterestPoint point)
    {
        if (!_interestPoints.Any(p => p.Id == point.Id))
        {
            _interestPoints.Add(point);
        }
    }

    public void RemoveInterestPoint(ulong id)
    {
        _interestPoints.RemoveAll(p => p.Id == id);
    }

    #endregion

    #region 私有方法

    private void CollectHighAttentionNodes(ISpacetimeNode node, List<ISpacetimeNode> result)
    {
        var attention = CalculateAttention(node.Bounds.Center, _playerPosition, _playerViewDirection);

        if (attention >= AttentionLevel.High.Value)
        {
            result.Add(node);
        }

        foreach (var child in node.Children)
        {
            CollectHighAttentionNodes(child, result);
        }
    }

    private void CollectNodesInViewport(ISpacetimeNode node, List<ISpacetimeNode> result)
    {
        var distance = node.Bounds.Center.DistanceTo(_playerPosition);

        if (distance < 1000)
        {
            result.Add(node);
        }

        foreach (var child in node.Children)
        {
            CollectNodesInViewport(child, result);
        }
    }

    #endregion
}

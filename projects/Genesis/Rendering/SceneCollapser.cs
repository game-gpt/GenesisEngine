using Genesis.Attention;
using Genesis.Core;
using Genesis.HAL;
using Genesis.Spacetime;

namespace Genesis.Rendering;

/// <summary>
/// 场景坍缩器
/// 将时空树中的节点坍缩为可渲染场景
/// </summary>
public sealed class SceneCollapser
{
    #region 字段

    private readonly ISpacetimeTree _spacetimeTree;
    private readonly IAttentionManager? _attentionManager;
    private readonly int _featureDimension;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化场景坍缩器
    /// </summary>
    /// <param name="spacetimeTree">时空树</param>
    /// <param name="attentionManager">注意力管理器</param>
    /// <param name="featureDimension">特征维度</param>
    public SceneCollapser(ISpacetimeTree spacetimeTree, IAttentionManager? attentionManager = null, int featureDimension = 8)
    {
        _spacetimeTree = spacetimeTree;
        _attentionManager = attentionManager;
        _featureDimension = featureDimension;
    }

    #endregion

    #region 公开方法

    /// <summary>
    /// 坍缩场景
    /// </summary>
    /// <param name="entityWorld">实体世界（HAL 抽象，替代直接依赖 Gnosis.ECS.World.World）</param>
    /// <param name="playerPosition">玩家位置</param>
    /// <param name="playerViewDirection">玩家视线方向</param>
    /// <returns>坍缩后的场景</returns>
    public ICollapsedScene Collapse(IEntityWorld entityWorld, Position playerPosition, Position playerViewDirection)
    {
        _attentionManager?.UpdateAttention(playerPosition, playerViewDirection);

        var visibleNodes = _attentionManager?.GetNodesInViewport() ?? GetAllNodes();
        var entities = new List<IRenderableEntity>();

        foreach (var node in visibleNodes)
        {
            var attention = _attentionManager?.CalculateAttention(node.Bounds.Center, playerPosition, playerViewDirection) ?? 1.0;

            var features = ExtractFeatures(node, attention);
            var entity = new RenderableEntity(
                new EntityId((uint)(node.SpatialHash & 0xFFFFFFFF), (uint)(node.SpatialHash >> 32)),
                $"SpacetimeNode_L{(int)node.Level}",
                node.Bounds.Center,
                features);

            entities.Add(entity);
        }

        var globalFeatures = ComputeGlobalFeatures(entities);
        var sceneHash = ComputeSceneHash(entities);
        var timestamp = Timestamp.Now;

        return new CollapsedScene(entities, globalFeatures, timestamp, sceneHash);
    }

    #endregion

    #region 私有方法

    private CausalFeatures ExtractFeatures(ISpacetimeNode node, double attention)
    {
        var values = new double[_featureDimension];
        values[0] = attention;
        values[1] = (double)node.Level / 3.0;
        values[2] = node.Bounds.SizeX;
        values[3] = node.Bounds.SizeY;
        values[4] = node.Bounds.SizeZ;
        values[5] = node.TimeScale / 86400.0;
        values[6] = node.CollapseState == Core.CollapseState.Collapsed ? 1.0 : 0.0;
        values[7] = (double)(node.HistoryHash % 1000) / 1000.0;

        return new CausalFeatures(values);
    }

    private CausalFeatures ComputeGlobalFeatures(IReadOnlyList<IRenderableEntity> entities)
    {
        if (entities.Count == 0)
        {
            return CausalFeatures.Zero(_featureDimension);
        }

        var sum = new double[_featureDimension];
        foreach (var entity in entities)
        {
            for (var i = 0; i < Math.Min(entity.Features.Dimension, _featureDimension); i++)
            {
                sum[i] += entity.Features[i];
            }
        }

        var avg = sum.Select(v => v / entities.Count).ToArray();
        return new CausalFeatures(avg).Normalize();
    }

    private static ulong ComputeSceneHash(IReadOnlyList<IRenderableEntity> entities)
    {
        ulong hash = 14695981039346656037;

        foreach (var entity in entities)
        {
            hash ^= ((ulong)entity.Id.Index << 32) | entity.Id.Generation;
            hash *= 1099511628211;
        }

        return hash;
    }

    private IReadOnlyList<ISpacetimeNode> GetAllNodes()
    {
        var result = new List<ISpacetimeNode>();

        if (_spacetimeTree.Root is not null)
        {
            CollectAllNodes(_spacetimeTree.Root, result);
        }

        return result.AsReadOnly();
    }

    private static void CollectAllNodes(ISpacetimeNode node, List<ISpacetimeNode> result)
    {
        result.Add(node);
        foreach (var child in node.Children)
        {
            CollectAllNodes(child, result);
        }
    }

    #endregion
}

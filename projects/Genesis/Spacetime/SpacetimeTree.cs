using Genesis.Core;

namespace Genesis.Spacetime;

/// <summary>
/// 时空树实现，管理时空节点的层级结构与索引
/// </summary>
public class SpacetimeTree : ISpacetimeTree
{
    #region 字段

    private ISpacetimeNode? _root;
    private readonly Dictionary<ulong, ISpacetimeNode> _nodeIndex;

    #endregion

    #region 属性

    /// <summary>
    /// 获取时空树的根节点
    /// </summary>
    public ISpacetimeNode? Root => _root;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化空的时空树
    /// </summary>
    public SpacetimeTree()
    {
        _root = null;
        _nodeIndex = new Dictionary<ulong, ISpacetimeNode>();
    }

    #endregion

    #region ISpacetimeTree 实现

    /// <summary>
    /// 根据空间哈希值获取节点
    /// </summary>
    /// <param name="spatialHash">空间哈希值</param>
    /// <returns>对应的时空节点，未找到则返回 null</returns>
    public ISpacetimeNode? GetNode(ulong spatialHash)
    {
        return _nodeIndex.TryGetValue(spatialHash, out var node) ? node : null;
    }

    /// <summary>
    /// 根据位置和层级在树中递归查找节点
    /// </summary>
    /// <param name="position">目标位置</param>
    /// <param name="level">目标层级</param>
    /// <returns>匹配的时空节点，未找到则返回 null</returns>
    public ISpacetimeNode? FindNode(Position position, NodeLevel level)
    {
        if (_root is null)
        {
            return null;
        }

        return FindNodeRecursive(_root, position, level);
    }

    /// <summary>
    /// 向时空树中插入节点，自动建立父子关系
    /// </summary>
    /// <param name="node">要插入的时空节点</param>
    public void InsertNode(ISpacetimeNode node)
    {
        _nodeIndex[node.SpatialHash] = node;

        if (_root is null && node.Level == NodeLevel.L3)
        {
            _root = node;
            return;
        }

        var parentLevel = (NodeLevel)((int)node.Level + 1);
        var parent = FindNode(node.Bounds.Center, parentLevel);

        if (parent is HChunkNode hChunkParent)
        {
            hChunkParent.AddChild(node);
        }
    }

    /// <summary>
    /// 根据空间哈希值从树中移除节点
    /// </summary>
    /// <param name="spatialHash">要移除节点的空间哈希值</param>
    public void RemoveNode(ulong spatialHash)
    {
        if (!_nodeIndex.Remove(spatialHash, out var node))
        {
            return;
        }

        if (ReferenceEquals(node, _root))
        {
            _root = null;
            return;
        }

        if (node is HChunkNode hChunkNode && hChunkNode.Parent is not null)
        {
            hChunkNode.Parent.RemoveChild(hChunkNode);
        }
    }

    /// <summary>
    /// 更新指定节点的历史哈希值，并沿父节点链向上级联更新
    /// </summary>
    /// <param name="spatialHash">要更新节点的空间哈希值</param>
    public void UpdateHistoryHash(ulong spatialHash)
    {
        var node = GetNode(spatialHash);
        if (node is null)
        {
            return;
        }

        node.UpdateHistoryHash();
        CascadeUpdateHistoryHash(node);
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 递归查找满足位置和层级条件的节点
    /// </summary>
    /// <param name="node">当前搜索节点</param>
    /// <param name="position">目标位置</param>
    /// <param name="level">目标层级</param>
    /// <returns>匹配的时空节点，未找到则返回 null</returns>
    private ISpacetimeNode? FindNodeRecursive(ISpacetimeNode node, Position position, NodeLevel level)
    {
        if (!node.Bounds.Contains(position))
        {
            return null;
        }

        if (node.Level == level)
        {
            return node;
        }

        foreach (var child in node.Children)
        {
            var result = FindNodeRecursive(child, position, level);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// 沿父节点链向上级联更新历史哈希值
    /// </summary>
    /// <param name="node">起始节点</param>
    private void CascadeUpdateHistoryHash(ISpacetimeNode node)
    {
        var current = node as HChunkNode;

        while (current?.Parent is not null)
        {
            current.Parent.UpdateHistoryHash();
            current = current.Parent;
        }
    }

    #endregion
}

using Genesis.Core.Enums;
using Genesis.Core.ValueObjects;
using Genesis.Spacetime.Interfaces;

namespace Genesis.Spacetime.Implementations;

/// <summary>
/// 层级区块节点，实现时空树中的空间分区与历史哈希链
/// </summary>
public class HChunkNode : ISpacetimeNode
{
    #region 私有字段

    private readonly ulong _spatialHash;
    private readonly HistoryHashChain _historyHashChain;
    private readonly NodeLevel _level;
    private readonly Bounds _bounds;
    private readonly double _timeScale;
    private CollapseState _collapseState;
    private readonly List<ISpacetimeNode> _children;
    private bool _isDirty;
    private bool _isCacheValid;
    private HChunkNode? _parent;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化层级区块节点
    /// </summary>
    /// <param name="spatialHash">空间哈希值</param>
    /// <param name="level">节点层级</param>
    /// <param name="bounds">空间边界</param>
    /// <param name="worldSeed">世界种子，非零时根据位置、层级和种子重新计算空间哈希</param>
    public HChunkNode(ulong spatialHash, NodeLevel level, Bounds bounds, ulong worldSeed = 0)
    {
        _level = level;
        _bounds = bounds;
        _timeScale = ComputeTimeScale(level);
        _collapseState = CollapseState.Superposition;
        _children = new List<ISpacetimeNode>();
        _isDirty = false;
        _isCacheValid = true;
        _parent = null;

        if (worldSeed != 0)
        {
            _spatialHash = SpatialHasher.ComputeSpatialHash(bounds.Center, level, worldSeed);
        }
        else
        {
            _spatialHash = spatialHash;
        }

        _historyHashChain = new HistoryHashChain();
    }

    #endregion

    #region ISpacetimeNode 属性

    /// <summary>
    /// 获取空间哈希值
    /// </summary>
    public ulong SpatialHash => _spatialHash;

    /// <summary>
    /// 获取历史哈希值
    /// </summary>
    public ulong HistoryHash => _historyHashChain.Value;

    /// <summary>
    /// 获取节点层级
    /// </summary>
    public NodeLevel Level => _level;

    /// <summary>
    /// 获取空间边界
    /// </summary>
    public Bounds Bounds => _bounds;

    /// <summary>
    /// 获取时间缩放因子
    /// </summary>
    public double TimeScale => _timeScale;

    /// <summary>
    /// 获取坍缩状态
    /// </summary>
    public CollapseState CollapseState => _collapseState;

    /// <summary>
    /// 获取子节点列表的只读视图
    /// </summary>
    public IReadOnlyList<ISpacetimeNode> Children => _children.AsReadOnly();

    #endregion

    #region 扩展属性

    /// <summary>
    /// 获取节点是否已标记为脏
    /// </summary>
    public bool IsDirty => _isDirty;

    /// <summary>
    /// 获取父节点
    /// </summary>
    public HChunkNode? Parent => _parent;

    #endregion

    #region ISpacetimeNode 方法

    /// <summary>
    /// 更新历史哈希值，若存在子节点则根据子节点计算组合哈希
    /// </summary>
    public void UpdateHistoryHash()
    {
        if (_children.Count > 0)
        {
            var computedHash = SpatialHasher.ComputeFromChildren(_children);
            _historyHashChain.Update(computedHash);
        }

        InvalidateCache();
    }

    /// <summary>
    /// 使缓存失效
    /// </summary>
    public void InvalidateCache()
    {
        _isCacheValid = false;
    }

    #endregion

    #region 子节点管理

    /// <summary>
    /// 添加子节点，并设置子节点的父引用
    /// </summary>
    /// <param name="child">要添加的子节点</param>
    public void AddChild(ISpacetimeNode child)
    {
        _children.Add(child);

        if (child is HChunkNode chunkChild)
        {
            chunkChild._parent = this;
        }

        MarkDirty();
    }

    /// <summary>
    /// 移除子节点，并清除子节点的父引用
    /// </summary>
    /// <param name="child">要移除的子节点</param>
    public void RemoveChild(ISpacetimeNode child)
    {
        _children.Remove(child);

        if (child is HChunkNode chunkChild)
        {
            chunkChild._parent = null;
        }

        MarkDirty();
    }

    #endregion

    #region 状态管理

    /// <summary>
    /// 将节点标记为脏
    /// </summary>
    public void MarkDirty()
    {
        _isDirty = true;
    }

    /// <summary>
    /// 将节点标记为干净
    /// </summary>
    public void MarkClean()
    {
        _isDirty = false;
    }

    /// <summary>
    /// 将节点状态设置为已坍缩
    /// </summary>
    public void SetCollapsed()
    {
        _collapseState = CollapseState.Collapsed;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 根据节点层级计算时间缩放因子
    /// </summary>
    /// <param name="level">节点层级</param>
    /// <returns>对应层级的时间缩放值</returns>
    private static double ComputeTimeScale(NodeLevel level)
    {
        return level switch
        {
            NodeLevel.L0 => 1.0,
            NodeLevel.L1 => 60.0,
            NodeLevel.L2 => 3600.0,
            NodeLevel.L3 => 86400.0,
            _ => 1.0
        };
    }

    #endregion
}

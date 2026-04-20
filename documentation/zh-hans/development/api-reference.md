# API 参考

本文档介绍 Genesis 引擎的核心 API。

## SpacetimeNode API

`ISpacetimeNode` 是时空节点的基础接口，代表层级区块树中的一个节点。

### 接口定义

```csharp
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
```

### 属性说明

| 属性 | 类型 | 说明 |
|------|------|------|
| `SpatialHash` | `ulong` | 基于空间位置与层级的唯一标识 |
| `HistoryHash` | `ulong` | 反映区域状态变化历史的哈希值 |
| `Level` | `NodeLevel` | 节点层级（L0~L3） |
| `Bounds` | `Bounds` | 空间边界 |
| `TimeScale` | `double` | 时间缩放因子 |
| `CollapseState` | `CollapseState` | 叠加态或坍缩态 |
| `Children` | `IReadOnlyList<ISpacetimeNode>` | 子节点集合 |

### 使用示例

```csharp
public void ProcessNode(ISpacetimeNode node)
{
    if (node.CollapseState == CollapseState.Superposition)
    {
        Console.WriteLine($"节点处于叠加态，层级: {node.Level}");
    }
    
    node.UpdateHistoryHash();
    node.InvalidateCache();
}
```

## CollapseEngine API

坍缩引擎负责将叠加态区块坍缩为确定性数据。

### 核心方法

```csharp
public interface ICollapseEngine
{
    ISpacetimeNode Collapse(ISpacetimeNode node, ulong seed);
    void PropagateConstraints(ISpacetimeNode node, BitMask[] masks);
    bool ValidateConsistency(ISpacetimeNode node, MemoryAnchor anchor);
}
```

### 坍缩示例

```csharp
public class CollapseService
{
    private readonly ICollapseEngine _engine;
    
    public ISpacetimeNode CollapseRegion(ISpacetimeNode node, ulong worldSeed)
    {
        var seed = HashCombine(node.HistoryHash, worldSeed);
        return _engine.Collapse(node, seed);
    }
    
    private static ulong HashCombine(ulong h1, ulong h2)
    {
        return h1 ^ (h2 + 0x9e3779b97f4a7c15 + (h1 << 6) + (h1 >> 2));
    }
}
```

## AttentionManager API

注意力管理器负责调度计算资源，实现注意力驱动计算。

### 核心接口

```csharp
public interface IAttentionManager
{
    IReadOnlySet<RegionId> DirtyRegions { get; }
    IReadOnlyList<Position> InterestPoints { get; }
    void UpdatePlayerPosition(Position position);
    void MarkDirty(RegionId region);
    void AddInterestPoint(Position point);
    void RemoveInterestPoint(Position point);
    BudgetAllocation AllocateBudget(TimeSpan frameBudget);
}
```

### 调度示例

```csharp
public class SimulationScheduler
{
    private readonly IAttentionManager _attention;
    
    public void OnFrame(TimeSpan frameBudget)
    {
        var allocation = _attention.AllocateBudget(frameBudget);
        
        foreach (var region in allocation.L0Regions)
        {
            UpdateL0Region(region);
        }
        
        foreach (var region in allocation.L1Regions)
        {
            UpdateL1Region(region);
        }
    }
}
```

## CausalAnchor API

因果锚点保证坍缩结果与玩家记忆一致。

### 接口定义

```csharp
public interface ICausalAnchor
{
    EntityId Id { get; }
    Position Position { get; }
    Timestamp CreatedAt { get; }
    IReadOnlyDictionary<string, object> Memory { get; }
    void Record(string key, object value);
    bool Validate(ISpacetimeNode node);
}
```

### 使用示例

```csharp
public class MemorySystem
{
    private readonly List<ICausalAnchor> _anchors = new();
    
    public void OnPlayerInteract(Position pos, string action)
    {
        var anchor = CreateAnchor(pos);
        anchor.Record("action", action);
        anchor.Record("timestamp", DateTime.UtcNow);
        _anchors.Add(anchor);
    }
    
    public bool ValidateCollapse(ISpacetimeNode node)
    {
        return _anchors.All(a => a.Validate(node));
    }
}
```

## 值对象

### Position

```csharp
public readonly record struct Position(int X, int Y, int Z);
```

### Bounds

```csharp
public readonly record struct Bounds(Position Min, Position Max);
```

### Timestamp

```csharp
public readonly record struct Timestamp(long Ticks);
```

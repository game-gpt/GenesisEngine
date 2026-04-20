# 层级区块实现

## 概述

层级区块（Hierarchical Chunk）是 Genesis 引擎的核心数据结构，将传统平面分块升级为四叉树（2D）或八叉树（3D）结构，实现空间划分、时间跳跃、概率态与因果哈希的统一管理。

## 区块节点结构

### 核心定义

```csharp
public class HChunkNode {
    public ulong SpatialHash { get; private set; }
    public ulong HistoryHash { get; private set; }
    public int Level { get; private set; }
    public Bounds Bounds { get; private set; }
    public HChunkNode[] Children { get; private set; }
    public Cell[] DeterministicData { get; private set; }
    public WaveFunctionState WaveState { get; private set; }
    private Dictionary<float, ulong> _evolutionCache;
}
```

### 字段说明

| 字段 | 类型 | 说明 |
|------|------|------|
| `SpatialHash` | `ulong` | 基于空间位置与层级的唯一标识 |
| `HistoryHash` | `ulong` | 反映从初始到当前的所有状态变化 |
| `Level` | `int` | 层级（0 为最细粒度） |
| `Bounds` | `Bounds` | 空间边界 |
| `Children` | `HChunkNode[]` | 子节点（展开时） |
| `DeterministicData` | `Cell[]` | 确定性数据（坍缩态 L0） |
| `WaveState` | `WaveFunctionState` | 概率云状态（叠加态） |

## 层级划分

### 层级定义

| 层级 | 精度 | 更新频率 | 典型内容 |
|------|------|----------|----------|
| **L0** | 全精度 | 60 Hz | 玩家、射弹、交互物体 |
| **L1** | 近场活跃 | 20 Hz | 视野边缘怪物、液体、火焰 |
| **L2** | 中场概率云 | 按需坍缩 | 未加载洞穴、远方森林 |
| **L3** | 远场宏观 | 极长周期 | 大陆漂移、生态区变迁 |

### 数据结构对比

| 层级 | 存储形式 | 内存占用 |
|------|----------|----------|
| L0 | 平坦数组/稀疏体素 | 高 |
| L1 | 16x16 区块 | 中 |
| L2 | WFC 叠加态 + 哈希 | 低 |
| L3 | 纯函数式 + 宏观参数 | 极低 |

## 层级转换

### L2 → L1 坍缩

触发 WFC 坍缩，将概率云转换为确定性数据：

```csharp
public HChunkNode CollapseToL1(HChunkNode l2Node) {
    var l1Node = new HChunkNode { Level = 1 };
    l1Node.DeterministicData = WFC.Collapse(
        l2Node.WaveState, 
        l2Node.HistoryHash
    );
    l1Node.WaveState = null;
    return l1Node;
}
```

### L1 → L2 模糊化

玩家远离时，将确定性数据模糊化为叠加态：

```csharp
public HChunkNode BlurToL2(HChunkNode l1Node) {
    var l2Node = new HChunkNode { Level = 2 };
    l2Node.WaveState = WFC.CreateFromDeterministic(
        l1Node.DeterministicData
    );
    l2Node.HistoryHash = l1Node.HistoryHash;
    return l2Node;
}
```

## 状态传播

### 历史哈希级联更新

玩家修改世界时，变化向上传播更新所有父节点：

```csharp
void OnCellChanged(HChunkNode leaf, int localX, int localY) {
    leaf.UpdateHistoryHash();
    var parent = leaf.Parent;
    while (parent != null) {
        parent.HistoryHash = ComputeHashFromChildren(parent.Children);
        parent.InvalidateCache();
        parent = parent.Parent;
    }
}
```

### 复杂度分析

| 操作 | 时间复杂度 |
|------|------------|
| 单次修改传播 | O(log N) |
| 脏标记批量更新 | O(脏区块数 × log N) |

## 内存管理

### 节点大小估算

| 组件 | 大小 |
|------|------|
| 基础字段 | ~64 字节 |
| 子节点引用 | 32/64 字节（2D/3D） |
| 确定性数据 | 可变（L0 最大） |
| WFC 状态 | 可变（L2 较小） |

### 内存优化策略

| 策略 | 说明 |
|------|------|
| 延迟加载 | 子节点按需展开 |
| 磁盘换出 | 非活跃节点持久化 |
| 引用共享 | 相同模式节点共享数据 |

### 内存占用估算

对于 10^12 单元的体素世界：

| 项目 | 数量 | 内存 |
|------|------|------|
| 八叉树节点 | ~10^8 | ~6.4 GB |
| L0 活跃区块 | ~100 | < 100 MB |
| HashLife 缓存 | - | < 512 MB |

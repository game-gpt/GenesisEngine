# 时空四维树 (Spacetime Four-Dimensional Tree)

## 概述

时空四维树是 Genesis 引擎的核心数据结构，将世界组织为层级化的空间树结构，并支持时间维度的快速演化。

## 节点结构 (HChunkNode)

每个节点代表一个空间范围，存储聚合信息与演化缓存：

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

| 字段 | 说明 |
|:---|:---|
| `SpatialHash` | 基于空间位置与层级的唯一标识 |
| `HistoryHash` | 反映从初始到当前的所有状态变化 |
| `Level` | 层级（0 为最细粒度） |
| `DeterministicData` | 坍缩态的确定性数据 |
| `WaveState` | 叠加态的概率云状态 |

## 层级体系 (L0-L4)

| 层级 | 数据结构 | 更新频率 | 典型内容 |
|:---|:---|:---|:---|
| **L0** | 平坦数组/稀疏体素 | 60Hz | 玩家、射弹、交互物体 |
| **L1** | 16x16 区块 | 20Hz | 视野边缘怪物、液体、火焰 |
| **L2** | WFC 叠加态 | 按需坍缩 | 未加载洞穴、远方森林 |
| **L3** | 纯函数式 | 极长周期 | 生态区变迁、宏观参数 |
| **L4** | 历史哈希 | 离线演化 | 大陆板块、文明更迭 |

## 时间膨胀机制

不同层级采用不同的时间流速：

- **L0**：实时更新，1:1 时间比例
- **L1**：降频更新，时间比例约 1:3
- **L2**：按需演化，可跳跃任意时间跨度
- **L3+**：宏观演化，以游戏日/月为单位

```csharp
ulong evolvedHash = HashCombine(node.HistoryHash, deltaTime, worldSeed);
var pseudoRandom = new DeterministicRandom(evolvedHash);
```

## 历史哈希 (Merkle 链)

历史哈希形成类似 Merkle 树的结构：

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

**特性**：
- 级联更新复杂度：O(log N)
- 支持快速状态验证
- 便于多人同步（仅传输哈希 + 时间戳）

## 空间哈希计算

使用 **xxHash64** 作为主哈希算法：

```csharp
ulong CombineHash(ulong h1, ulong h2) {
    return h1 ^ (h2 + 0x9e3779b97f4a7c15 + (h1 << 6) + (h1 >> 2));
}
```

**空间哈希组成**：
1. 世界坐标 (x, y, z)
2. 层级深度
3. 世界种子

## HashLife 缓存

缓存结构：`Dictionary<(ulong nodeHash, float deltaTime), ulong resultHash>`

| 策略 | 说明 |
|:---|:---|
| LRU 淘汰 | 优先淘汰高层级节点缓存 |
| 内存限制 | 通常限制在 512MB 以内 |
| 查询复杂度 | O(1) 哈希查表 |

## 层级转换

- **L2 → L1**：触发 WFC 坍缩，概率云转为确定性数据
- **L1 → L2**：玩家远离时，确定性数据模糊化为叠加态

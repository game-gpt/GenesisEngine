# HashLife 缓存机制

## 概述

HashLife 是一种利用哈希表缓存元胞自动机演化结果的算法。Genesis 引擎将其推广到更复杂的游戏规则系统，实现时间复杂度从 O(Δt) 降至 O(log Δt)。

## HashLife 算法原理

### 核心思想

HashLife 的核心是**用空间换时间**：将不同尺度、不同时间步长的演化结果缓存到哈希表中。当相同模式的区块再次需要演化时，直接返回缓存结果。

### 适用条件

| 条件 | 说明 |
|------|------|
| 确定性 | 相同输入必得相同输出 |
| 局部性 | 演化只依赖邻域状态 |
| 固定步长 | 时间步长一致 |

## 时间复杂度分析

传统逐帧模拟的时间复杂度为 O(Δt)，即演化时间与时间跨度成正比。

HashLife 通过层级缓存实现：

```
演化时间 = O(log Δt)
```

| 时间跨度 | 传统模拟 | HashLife |
|----------|----------|----------|
| 1 帧 | 1 次计算 | 1 次计算 |
| 16 帧 | 16 次计算 | 4 次查表 |
| 1024 帧 | 1024 次计算 | 10 次查表 |
| 65536 帧 | 65536 次计算 | 16 次查表 |

## 缓存结构

### 数据结构

```csharp
public class HashLifeCache {
    private Dictionary<(ulong nodeHash, float deltaTime), ulong> _cache;
    private int _maxSize;
    private LinkedList<(ulong, float)> _lruList;
}
```

### 缓存键值

| 字段 | 类型 | 说明 |
|------|------|------|
| `nodeHash` | `ulong` | 区块节点的空间与历史哈希组合 |
| `deltaTime` | `float` | 时间步长 |
| `resultHash` | `ulong` | 演化后节点的哈希 |

## LRU 淘汰策略

### 淘汰优先级

内存受限时采用 LRU（最近最少使用）淘汰：

1. **优先淘汰高层级节点**：高层级演化成本低，重新计算开销小
2. **保留低层级热点**：玩家活跃区域频繁访问，保留缓存收益大

### 实现代码

```csharp
public ulong GetOrCompute(ulong nodeHash, float deltaTime, Func<ulong> compute) {
    var key = (nodeHash, deltaTime);
    if (_cache.TryGetValue(key, out var result)) {
        UpdateLRU(key);
        return result;
    }
    
    if (_cache.Count >= _maxSize) {
        EvictLRU();
    }
    
    result = compute();
    _cache[key] = result;
    _lruList.AddLast(key);
    return result;
}
```

## 游戏规则应用

### 确定性规则

| 规则类型 | 缓存效果 | 示例 |
|----------|----------|------|
| 沙子下落 | 完全缓存 | 8x8 沙堆 1 秒后形态确定 |
| 水流动 | 完全缓存 | 封闭洞穴内水位平衡 |
| 红石电路 | 真值表缓存 | 组合逻辑输出 |

### 涌现性规则

对于非确定性规则，使用**种子哈希 + 伪随机**模拟确定性：

```csharp
ulong evolvedHash = HashCombine(node.HistoryHash, deltaTime, worldSeed);
var pseudoRandom = new DeterministicRandom(evolvedHash);
```

## 性能参数

| 参数 | 推荐值 | 说明 |
|------|--------|------|
| 最大缓存大小 | 512 MB | 平衡内存与命中率 |
| 缓存命中率目标 | > 80% | L0/L1 层级 |
| 查询时间 | < 1 μs | 哈希表 O(1) |

## 哈希函数选择

Genesis 使用 **xxHash64** 作为主要哈希算法：

```csharp
ulong CombineHash(ulong h1, ulong h2) {
    return h1 ^ (h2 + 0x9e3779b97f4a7c15 + (h1 << 6) + (h1 >> 2));
}
```

选择理由：
- 极高的计算速度
- 良好的分布性
- 低碰撞率

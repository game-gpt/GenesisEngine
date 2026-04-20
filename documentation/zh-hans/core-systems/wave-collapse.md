# 波函数坍缩 (Wave Function Collapse)

## 概述

波函数坍缩（WFC）是 Genesis 引擎的核心算法，用于世界生成和动态场景的按需具现化。

## WFC 算法基础

### 核心思想

1. 每个格子有一组**可能的状态**
2. 选择**熵最小**的格子，随机确定其状态（坍缩）
3. 将新状态的约束传播给邻居
4. 重复直到所有格子确定

### 状态表示

```csharp
public class WaveFunctionState {
    public BitMask[] CellMasks;
    public List<GlobalConstraint> Constraints;
    public ulong CollapseSeed;
    public bool IsCollapsed;
}
```

| 字段 | 说明 |
|:---|:---|:---|
| `CellMasks` | 每个单元格的可能状态位图 |
| `Constraints` | 全局约束列表 |
| `CollapseSeed` | 由历史哈希派生的随机种子 |
| `IsCollapsed` | 是否已完成坍缩 |

## 坍缩过程

### 从概率云到确定性

```
叠加态（概率云）
      │
      ▼ 选择熵最小格子
   ┌─────┐
   │采样 │ ← 基于历史哈希的确定性随机
   └─────┘
      │
      ▼ 约束传播
   ┌─────┐
   │更新 │ ← 更新邻居可能性集合
   └─────┘
      │
      ▼ 重复
   ┌─────┐
   │坍缩 │ → 确定性状态
   └─────┘
```

### 坍缩伪代码

```csharp
void Collapse(WaveFunctionState state) {
    var random = new DeterministicRandom(state.CollapseSeed);
    while (!state.IsFullyCollapsed()) {
        var cell = state.FindMinEntropyCell();
        var chosenState = random.Sample(cell.PossibleStates);
        cell.CollapseTo(chosenState);
        PropagateConstraints(state, cell);
    }
}
```

## 因果一致性保证

### 记忆锚点机制

玩家交互过的区域标记为**记忆锚点**，作为固定约束：

```csharp
void ApplyMemoryAnchors(WaveFunctionState state, List<MemoryAnchor> anchors) {
    foreach (var anchor in anchors) {
        state.FixRegion(anchor.Region, anchor.DeterministicState);
    }
}
```

### 自洽性验证

```csharp
bool VerifyConsistency(CollapsedState state, PlayerMemory memory) {
    foreach (var remembered in memory.Interactions) {
        if (state.GetCell(remembered.Position) != remembered.ExpectedState)
            return false;
    }
    return true;
}
```

## 优化策略

| 策略 | 说明 |
|:---|:---|
| **增量坍缩** | 随玩家视线移动逐步坍缩 |
| **低分辨率坍缩** | LOD 层级使用更粗粒度 |
| **预坍缩** | 后台预测玩家移动方向 |
| **波前并行** | 多线程并行约束传播 |

### 波前并行算法

```csharp
void ParallelPropagate(WaveFunctionState state) {
    var queue = new ConcurrentQueue<Cell>();
    Parallel.ForEach(queue, cell => {
        var newMask = ComputeMaskFromNeighbors(cell);
        if (Interlocked.CompareExchange(ref cell.Mask, newMask, cell.Mask) != cell.Mask) {
            foreach (var neighbor in cell.Neighbors)
                queue.Enqueue(neighbor);
        }
    });
}
```

## 生成管线

```
宏观层（噪声）→ 中观层（WFC 地形）→ 微观层（二次坍缩）→ 装饰层
```

| 层级 | 方法 | 内容 |
|:---|:---|:---|
| 宏观 | Perlin/Simplex 噪声 | 大陆、海洋、气候带 |
| 中观 | WFC | 山脉、河流、洞穴入口 |
| 微观 | 二次 WFC 坍缩 | 植被、矿物、小结构 |
| 装饰 | 历史哈希 | 宝箱、刷新点 |

## 边界约束处理

相邻区块采用**重叠约束传播**：

```csharp
void GenerateAdjacent(Chunk current, Chunk neighbor) {
    var edgeConstraints = current.GetEdgeStates();
    neighbor.WFCState.ApplyBoundaryConstraints(edgeConstraints);
    neighbor.Collapse();
}
```

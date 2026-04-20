# 注意力调度 (Attention Scheduling)

## 概述

注意力调度是 Genesis 引擎的核心调度机制，基于玩家注意力分配计算资源，实现高效的层级化模拟。

## 注意力计算公式

```csharp
float CalculateAttentionScore(Vector3 position, PlayerState player) {
    float distanceScore = 1.0f / (1.0f + Vector3.Distance(position, player.Position));
    float viewScore = IsInView(position, player.ViewFrustum) ? 1.0f : 0.3f;
    float interactionScore = IsInInteractionRange(position, player) ? 2.0f : 1.0f;
    float interestScore = GetInterestPointWeight(position);
    
    return distanceScore * viewScore * interactionScore * interestScore;
}
```

| 因素 | 权重影响 |
|:---|:---|
| 距离 | 反比衰减 |
| 视野 | 视野内 ×1.0，视野外 ×0.3 |
| 交互范围 | 可触及 ×2.0 |
| 兴趣点 | 额外加成 |

## 脏区域追踪

### 数据结构

```csharp
public class DirtyRegionTracker {
    private HashSet<HChunkNode> _dirtyNodes;
    private Dictionary<int, List<HChunkNode>> _dirtyByLevel;
    private Queue<HChunkNode> _propagationQueue;
}
```

### 脏标记传播

```csharp
void MarkDirty(HChunkNode node) {
    node.IsDirty = true;
    _dirtyNodes.Add(node);
    _dirtyByLevel[node.Level].Add(node);
    
    if (node.Level > 0) {
        foreach (var child in node.Children)
            MarkDirty(child);
    }
}
```

### 延迟批量更新

玩家操作导致的脏标记先累积在叶节点，帧末统一向上传播哈希更新：

```csharp
void FlushDirtyMarks() {
    foreach (var node in _dirtyNodes.Where(n => n.Level == 0)) {
        PropagateHashToParent(node);
    }
    _dirtyNodes.Clear();
}
```

## LOD 管理

### 层级精度分配

| 层级 | 精度 | 更新策略 |
|:---|:---|:---|
| **L0** | 全精度 | 每帧更新（60Hz） |
| **L1** | 高精度 | 降频更新（20Hz） |
| **L2** | 概率云 | 按需坍缩 |
| **L3** | 宏观参数 | 极低频率 |

### LOD 转换条件

```csharp
void UpdateLOD(HChunkNode node, float attentionScore) {
    if (attentionScore > 0.8f && node.Level > 0) {
        node.Subdivide();
    } else if (attentionScore < 0.1f && node.Level < MaxLevel) {
        node.CollapseToParent();
    }
}
```

### 计算预算分配

每帧 16ms 预算分配顺序：

1. **L0 脏区域**：必须全部更新
2. **L0 视野内非脏区域**：选择性更新（每 3 帧）
3. **L1 兴趣点**：预算有余时更新
4. **L2 背景演化**：极低频率或睡眠时执行

## 兴趣点机制

### 兴趣点类型

| 类型 | 触发条件 | 权重 |
|:---|:---|:---|
| **基地** | 玩家建筑/存储 | 3.0 - 10.0 |
| **任务目标** | 任务系统标记 | 2.0 - 8.0 |
| **刷怪区域** | 活跃怪物生成点 | 1.5 - 5.0 |
| **玩家标记** | 地图标记/路标 | 1.0 - 3.0 |

### 兴趣点管理

```csharp
public class InterestPointManager {
    private List<InterestPoint> _points;
    
    public void AddPoint(Vector3 position, float weight, InterestType type) {
        _points.Add(new InterestPoint {
            Position = position,
            Weight = weight,
            Type = type,
            CreatedTime = Time.Now
        });
    }
    
    public float GetWeightAt(Vector3 position) {
        return _points
            .Where(p => Vector3.Distance(p.Position, position) < p.Radius)
            .Sum(p => p.Weight);
    }
}
```

## 时间膨胀

玩家在基地内活动时，周围生态演化速度可人为调慢；远离区域可时间加速：

```csharp
float GetTimeScale(Vector3 position, PlayerState player) {
    float distance = Vector3.Distance(position, player.Position);
    if (distance < BaseRadius) return 0.5f;  // 基地内减速
    if (distance > FarDistance) return 2.0f; // 远方加速
    return 1.0f;
}
```

## 调度器核心循环

```
每帧执行：
1. 收集输入，更新玩家位置、视角
2. 注意力管理器更新脏区域、兴趣点、视锥
3. 调度器分配预算：
   a. 更新所有 L0 脏区块
   b. 坍缩新进入视野的叠加态区块
   c. 执行 HashLife 预演算（后台线程）
4. 渲染层提取确定性数据并绘制
```

## 性能优化

| 优化项 | 方法 |
|:---|:---|
| 空间索引 | 八叉树快速剔除 |
| 预测加载 | 根据移动方向预坍缩 |
| 后台演化 | HashLife 异步计算 |
| 内存控制 | LRU 缓存淘汰 |

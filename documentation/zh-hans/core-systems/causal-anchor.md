# 因果锚点系统 (Causal Anchor System)

## 概述

因果锚点是 Genesis 引擎保证世界自洽性的核心机制，确保波函数坍缩结果与玩家已有记忆一致。

## 锚点定义

因果锚点是玩家交互过的区域标记，存储确定性数据，永不进入叠加态：

```csharp
public class CausalAnchor {
    public ulong AnchorId { get; private set; }
    public Bounds Region { get; private set; }
    public float Weight { get; private set; }
    public ulong StateHash { get; private set; }
    public DateTime LastInteraction { get; private set; }
    public AnchorType Type { get; private set; }
}
```

| 字段 | 说明 |
|:---|:---|
| `AnchorId` | 唯一标识符 |
| `Region` | 锚点覆盖的空间范围 |
| `Weight` | 锚点权重（影响坍缩优先级） |
| `StateHash` | 确定性状态的哈希值 |
| `Type` | 锚点类型（记忆/兴趣点/任务） |

## 锚点类型

| 类型 | 触发条件 | 权重范围 |
|:---|:---|:---|
| **交互记忆** | 玩家修改/使用物体 | 1.0 - 10.0 |
| **兴趣点** | 玩家标记位置 | 0.5 - 5.0 |
| **任务目标** | 任务系统创建 | 2.0 - 8.0 |
| **基地区域** | 玩家频繁活动区 | 3.0 - 15.0 |

## 权重累积机制

权重随交互次数和时间衰减：

```csharp
float CalculateWeight(CausalAnchor anchor) {
    float baseWeight = anchor.BaseWeight;
    int interactionCount = anchor.InteractionCount;
    float timeDecay = Math.Exp(-TimeSinceLastInteraction / DecayConstant);
    return baseWeight * interactionCount * timeDecay;
}
```

**权重影响因素**：
- 交互频率：每次交互增加权重
- 时间衰减：随时间指数衰减
- 距离因素：远离时权重降低

## 锚点纠缠

相邻锚点形成纠缠关系，坍缩时相互约束：

```csharp
public class AnchorEntanglement {
    public CausalAnchor AnchorA { get; private set; }
    public CausalAnchor AnchorB { get; private set; }
    public float EntanglementStrength { get; private set; }
    public ConstraintType Constraint { get; private set; }
}
```

| 约束类型 | 说明 |
|:---|:---|
| `Spatial` | 空间连续性约束 |
| `Temporal` | 时间因果约束 |
| `Semantic` | 语义逻辑约束 |

## 对坍缩的影响

锚点在 WFC 坍缩中作为固定约束：

```csharp
void CollapseWithAnchors(WaveFunctionState state, List<CausalAnchor> anchors) {
    foreach (var anchor in anchors.OrderByDescending(a => a.Weight)) {
        state.ApplyConstraint(anchor.Region, anchor.StateHash);
    }
    state.PropagateConstraints();
    state.CollapseRemaining();
}
```

**坍缩优先级**：
1. 高权重锚点优先应用
2. 纠缠锚点联合约束
3. 剩余区域自由坍缩

## 记忆锚点生命周期

```
创建 → 活跃 → 衰减 → 归档
 │       │       │       │
交互触发  持续交互  远离区域  权重归零
```

## 与世界持久化的关系

| 操作 | 处理方式 |
|:---|:---|
| 存档 | 序列化所有活跃锚点 |
| 读档 | 恢复锚点并验证哈希 |
| 多人同步 | 广播锚点状态变更 |

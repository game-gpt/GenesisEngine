# 快速开始

## 环境要求

### 必需条件

| 工具 | 版本要求 | 说明 |
|------|----------|------|
| .NET SDK | 8.0+ | 核心运行时 |
| IDE | Rider / VS 2022 | 推荐使用 Rider |

### 可选条件

| 工具 | 用途 |
|------|------|
| Docker | 容器化部署 |
| Git | 版本控制 |

## 项目结构

```
GenesisEngine/
├── projects/
│   └── Genesis/
│       ├── Core/           # 核心类型定义
│       │   ├── Enums/      # 枚举类型
│       │   ├── Events/     # 领域事件
│       │   ├── Interfaces/ # 核心接口
│       │   └── ValueObjects/ # 值对象
│       └── Spacetime/      # 时空四维树实现
│           ├── Interfaces/ # 时空接口
│           └── ValueObjects/ # 时空节点
└── documentation/          # 文档
```

## 基本概念

### 节点层级

Genesis 使用层级区块（Hierarchical Chunk）组织世界：

```csharp
public enum NodeLevel
{
    L0 = 0,  // 全精度确定性模拟
    L1 = 1,  // 近场活跃区
    L2 = 2,  // 中场概率云
    L3 = 3   // 远场宏观演化
}
```

### 坍缩状态

每个时空节点可处于两种状态：

```csharp
public enum CollapseState
{
    Superposition,  // 叠加态（概率云）
    Collapsed       // 坍缩态（确定性）
}
```

### 核心接口

#### 时空节点

```csharp
public interface ISpacetimeNode
{
    NodeLevel Level { get; }
    Bounds Bounds { get; }
    CollapseState State { get; }
    ulong HistoryHash { get; }
    IReadOnlyList<ISpacetimeNode> Children { get; }
}
```

#### 时空树

```csharp
public interface ISpacetimeTree
{
    ISpacetimeNode Root { get; }
    ISpacetimeNode GetNode(Position position, NodeLevel level);
    void Collapse(Position position);
}
```

## 简单示例

### 创建时空树

```csharp
var tree = SpacetimeTree.Create(
    seed: 12345,
    bounds: Bounds.FromCenter(Position.Zero, new Position(1024, 1024, 1024))
);
```

### 查询节点状态

```csharp
var node = tree.GetNode(playerPosition, NodeLevel.L0);

if (node.State == CollapseState.Superposition)
{
    tree.Collapse(playerPosition);
}
```

### 获取历史哈希

```csharp
ulong historyHash = node.HistoryHash;
```

历史哈希反映该区域从初始到当前时刻的所有状态变化，是因果追踪的基础。

## 规则类型

Genesis 将游戏规则分为两类：

| 类型 | 特点 | 计算模式 |
|------|------|----------|
| **确定性规则** | 输入输出严格对应；局部性；可缓存 | HashLife 加速 |
| **涌现性规则** | 统计性、随机性；非局部影响；可近似 | WFC 坍缩 + 伪随机 |

```csharp
public enum RuleType
{
    Deterministic,  // 确定性规则
    Emergent        // 涌现性规则
}
```

## 下一步

1. 阅读 [项目介绍](./introduction.md) 了解核心哲学
2. 探索 `projects/Genesis/` 目录下的源码
3. 查看架构设计文档（待完善）

## 常见问题

### Q: 为什么需要时空四维树？

传统数据结构无法同时满足：叠加态支持、时间跳跃、因果追踪、分层精度四个需求。时空四维树是唯一可行的数学结构。

### Q: 坍缩计算开销大吗？

通过注意力驱动的渐进坍缩、异步工作线程和预测性坍缩，可以有效控制开销。只有玩家注视的区域才会被高精度坍缩。

### Q: 如何保证多玩家一致性？

服务器只同步因果事件，渲染在客户端独立完成（主观坍缩）。因果哈希链保证历史不可篡改。

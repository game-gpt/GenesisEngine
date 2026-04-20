# Genesis 引擎项目文档

**因果引擎：为涌现式叙事与无限世界而生**

---

## 项目概述

Genesis 引擎是一套以**时空四维树**与**因果锚点系统**为核心的因果引擎，专为涌现式叙事、活态世界和玩家驱动的命运演化而设计。它从根本上放弃了传统游戏引擎"确定性快照 + 全局时间 + 预设因果标记"的架构，转而采用**概率云叠加态**、**注意力驱动的动态坍缩**以及**可追溯的 Merkle 因果哈希链**。

---

## 项目结构

```
Genesis/
├── Core/                    # 核心基础设施
│   ├── ValueObjects/        # 值对象：Position, RegionId, EntityId, Timestamp, Bounds
│   ├── Enums/               # 枚举：NodeLevel, CollapseState, RuleType
│   ├── Interfaces/          # 基础接口：IEntity, IComponent, ISystem
│   └── Events/              # 事件系统：IDomainEvent, DomainEventBase
│
├── Spacetime/               # 时空四维树
│   ├── Interfaces/          # ISpacetimeNode, ISpacetimeTree, IHistoryHash
│   └── ValueObjects/        # SpacetimeNode, TimeScale
│
├── Causal/                  # 因果锚点系统
│   ├── Interfaces/          # ICausalAnchor, ICausalWeightSystem, ICausalFeatureVector
│   ├── ValueObjects/        # CausalAnchor, CausalWeight
│   └── Enums/               # AnchorType
│
├── Collapse/                # 波函数坍缩
│   ├── Interfaces/          # IWaveFunctionState, ICollapser, IConstraint
│   ├── ValueObjects/        # WaveFunctionState
│   └── Enums/               # ConstraintType
│
├── Attention/               # 注意力调度
│   ├── Interfaces/          # IAttentionManager, IAttentionModel, IDirtyRegion
│   └── ValueObjects/        # AttentionLevel, DirtyRegion, InterestPoint
│
├── HashLife/                # HashLife 缓存
│   ├── Interfaces/          # IHashLifeCache, IEvolutionResult
│   └── ValueObjects/        # CacheEntry, EvolutionKey
│
├── Rules/                   # 规则系统
│   ├── Interfaces/          # IRule, IRuleEngine, IDeterministicRule, IEmergentRule
│   └── ValueObjects/        # RulePriority
│
├── Rendering/               # 渲染解耦层
│   ├── Interfaces/          # ICollapsedScene, IRenderableEntity
│   └── ValueObjects/        # CausalFeatures, SceneDescription
│
├── World/                   # 世界生成
│   ├── Interfaces/          # IWorldGenerator, INoiseGenerator, IWFCGenerator
│   ├── ValueObjects/        # WorldSeed
│   └── Enums/               # BiomeType
│
└── Persistence/             # 持久化层
    ├── Interfaces/          # IRepository<T>, IHistoryStore, IIncrementalSave
    └── Implementations/     # InMemoryHistoryStore
```

---

## 核心模块详解

### Core - 核心基础设施

提供整个引擎的基础数据类型和抽象。

**值对象**：
- `Position` - 三维坐标，支持距离计算
- `RegionId` - 区域唯一标识符
- `EntityId` - 实体唯一标识符
- `Timestamp` - 时间戳，支持 Unix 时间转换
- `Bounds` - 空间边界，支持包含检测

**枚举**：
- `NodeLevel` - 节点层级（L0-L3）
- `CollapseState` - 坍缩状态（Superposition/Collapsed）
- `RuleType` - 规则类型（Deterministic/Emergent）

---

### Spacetime - 时空四维树

引擎的核心数据结构，将空间和时间统一为不可分割的时空连续体。

**核心概念**：
- **时空节点**：每个节点代表一个空间范围和一个时间尺度
- **历史哈希**：Merkle 树结构，保证因果历史不可篡改
- **时间膨胀**：不同层级有不同的时间流速

| 层级 | 空间尺度 | 时间尺度 | 相对时间流速 |
|:---|:---|:---|:---|
| L0 | 1x1x1 体素 | 1 tick (16ms) | 1x（基准） |
| L1 | 8x8x8 | 1 秒 | 60x 加速 |
| L2 | 64x64x64 | 1 分钟 | 3600x 加速 |
| L3 | 512x512x512 | 1 小时 | 216000x 加速 |

---

### Causal - 因果锚点系统

管理玩家行为对世界命运的连续权重贡献。

**核心概念**：
- **因果锚点**：玩家行为的权重贡献记录
- **因果权重系统**：管理权重的累积、传播和纠缠
- **因果特征向量**：低维连续向量，总结实体的因果历史

**锚点类型**：
- Hope（希望）
- Wisdom（智慧）
- Chaos（混乱）
- Order（秩序）
- Exploration（探索）
- Combat（战斗）
- Trade（贸易）
- Culture（文化）

---

### Collapse - 波函数坍缩

实现世界状态从概率云到确定性的转换。

**核心概念**：
- **波函数状态**：每个单元格的可能状态掩码
- **坍缩器**：执行坍缩过程的算法
- **约束**：保证坍缩结果的自洽性

**坍缩过程**：
1. 选择熵最小的单元格
2. 随机确定其状态
3. 将约束传播给邻居
4. 重复直到所有单元格确定

---

### Attention - 注意力调度

管理玩家注意力作为稀缺计算资源的分配。

**核心概念**：
- **注意力管理器**：计算和分配注意力
- **注意力模型**：多维度注意力计算
- **脏区域**：记录发生变化的区块

**注意力计算**：
```
Attention = w1/distance + w2·view_dot + w3·interaction + w4·causal_importance
```

---

### HashLife - HashLife 缓存

实现时间跳跃优化的缓存系统。

**核心概念**：
- **缓存条目**：存储节点哈希、结果哈希、时间增量
- **演化键**：用于缓存查找的组合键

**性能优势**：
- 将演化计算从 O(Δt) 降为 O(log Δt)
- 支持确定性规则的直接缓存
- 支持涌现性规则的伪随机缓存

---

### Rules - 规则系统

管理世界演化的规则引擎。

**规则分类**：
- **确定性规则**：输入输出严格对应，可缓存
  - 沙子下落、水流动、红石逻辑
- **涌现性规则**：统计性、随机性，可近似
  - 草蔓延、火扩散、动物繁殖

**规则优先级**：
- Lowest (0)
- Low (25)
- Normal (50)
- High (75)
- Highest (100)

---

### Rendering - 渲染解耦层

提供因果层与表现层的解耦接口。

**核心概念**：
- **坍缩场景**：坍缩后的确定性场景描述
- **可渲染实体**：包含因果特征的实体
- **因果特征**：用于调制表现层的低维向量

**渲染后端选项**：
- 传统光栅化
- NeRF / 3D Gaussian Splatting
- 端到端扩散模型
- 混合渲染

---

### World - 世界生成

管理世界的初始化和区域生成。

**核心概念**：
- **世界生成器**：协调区域生成
- **噪声生成器**：生成地形噪声
- **WFC 生成器**：基于约束的地形生成

**生物群系类型**：
- Plains（平原）
- Forest（森林）
- Desert（沙漠）
- Mountains（山脉）
- Ocean（海洋）
- Tundra（冻原）
- Jungle（丛林）
- Swamp（沼泽）
- Volcanic（火山）
- Crystal（水晶）

---

### Persistence - 持久化层

管理世界状态的存储和恢复。

**核心概念**：
- **仓储**：实体的增删改查
- **历史存储**：因果哈希链的存储
- **增量保存**：检查点管理

---

## 技术规格

- **目标框架**: .NET 10.0
- **命名空间**: Genesis.{ModuleName}.{SubDirectory}
- **值对象**: 使用 record struct 类型
- **配置**: 启用 Nullable 和 ImplicitUsings

---

## 设计公理

1. **状态的不确定性是创造力的土壤。** 只有当世界状态存在由玩家行为决定的"不确定性"时，探索与试错才有意义，个人化的发现与创造才可能发生。

2. **社会共识是虚拟世界真实性的唯一锚点。** 一个物品之所以有价值，是因为玩家社群共同相信它有价值。一个事件之所以是"历史"，是因为它被玩家社群共同见证、共同记忆。

3. **知识资产是终极的价值存储。** 在现实世界，财富的终极形式是能够创造更多财富的知识与关系网络。在 Genesis 构建的虚拟世界中，这一规律同样适用。

---

## 相关文档

- [核心系统文档](../../documentation/zh-hans/core-systems/)
- [技术文档](../../documentation/zh-hans/technical/)
- [开发指南](../../documentation/zh-hans/development/)
- [游戏适配](../../documentation/zh-hans/game-adaptation/)

---

*"起初，世界是一片概率的虚空。玩家的目光是光，于是有了天地、山河、草木、虫豸。每一次回眸，都是一次新的创世。"*

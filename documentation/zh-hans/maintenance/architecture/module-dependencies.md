# 模块依赖关系

本文档描述 Genesis 引擎各模块之间的依赖关系。

---

## 模块层级

Genesis 引擎采用分层架构，模块依赖遵循自下而上的原则。

```
┌─────────────────────────────────────────────────────────┐
│                     应用层                              │
│  World · Rendering · Persistence                        │
├─────────────────────────────────────────────────────────┤
│                     业务层                              │
│  Causal · Collapse · Attention · HashLife · Rules       │
├─────────────────────────────────────────────────────────┤
│                     核心层                              │
│  Spacetime                                              │
├─────────────────────────────────────────────────────────┤
│                     基础层                              │
│  Core                                                   │
└─────────────────────────────────────────────────────────┘
```

---

## 依赖矩阵

| 模块 | Core | Spacetime | Causal | Collapse | Attention | HashLife | Rules | Rendering | World | Persistence |
|:---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Core** | - | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Spacetime** | ✅ | - | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Causal** | ✅ | ❌ | - | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Collapse** | ✅ | ✅ | ✅ | - | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Attention** | ✅ | ✅ | ❌ | ❌ | - | ❌ | ❌ | ❌ | ❌ | ❌ |
| **HashLife** | ✅ | ✅ | ❌ | ❌ | ❌ | - | ❌ | ❌ | ❌ | ❌ |
| **Rules** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | - | ❌ | ❌ | ❌ |
| **Rendering** | ✅ | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ | - | ❌ | ❌ |
| **World** | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | - | ❌ |
| **Persistence** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | - |

---

## 详细依赖说明

### Core - 基础层

**职责**：提供整个引擎的基础数据类型和抽象。

**依赖**：无外部依赖。

**被依赖**：所有其他模块。

**核心类型**：
- 值对象：`Position`, `RegionId`, `EntityId`, `Timestamp`, `Bounds`
- 枚举：`NodeLevel`, `CollapseState`, `RuleType`
- 接口：`IEntity`, `IComponent`, `ISystem`
- 事件：`IDomainEvent`, `DomainEventBase`

---

### Spacetime - 核心层

**职责**：时空四维树的核心实现。

**依赖**：
- `Core` - 使用基础值对象和枚举

**被依赖**：
- `Collapse` - 坍缩操作需要访问时空节点
- `Attention` - 注意力计算需要节点信息
- `HashLife` - 缓存需要节点哈希
- `Persistence` - 持久化需要存储节点状态

**核心类型**：
- 接口：`ISpacetimeNode`, `ISpacetimeTree`, `IHistoryHash`
- 值对象：`SpacetimeNode`, `TimeScale`

---

### Causal - 业务层

**职责**：因果锚点系统的实现。

**依赖**：
- `Core` - 使用 `PlayerId` 等基础类型

**被依赖**：
- `Collapse` - 坍缩需要考虑因果权重
- `Rendering` - 渲染需要因果特征向量

**核心类型**：
- 接口：`ICausalAnchor`, `ICausalWeightSystem`, `ICausalFeatureVector`
- 值对象：`CausalAnchor`, `CausalWeight`
- 枚举：`AnchorType`

---

### Collapse - 业务层

**职责**：波函数坍缩的实现。

**依赖**：
- `Core` - 使用基础枚举和值对象
- `Spacetime` - 访问时空节点
- `Causal` - 考虑因果权重

**被依赖**：
- `Rendering` - 渲染需要坍缩后的场景
- `World` - 世界生成需要坍缩

**核心类型**：
- 接口：`IWaveFunctionState`, `ICollapser`, `IConstraint`
- 值对象：`WaveFunctionState`
- 枚举：`ConstraintType`

---

### Attention - 业务层

**职责**：注意力调度的实现。

**依赖**：
- `Core` - 使用 `Position` 等值对象
- `Spacetime` - 访问节点信息

**被依赖**：无直接被依赖，通过事件机制与其他模块交互。

**核心类型**：
- 接口：`IAttentionManager`, `IAttentionModel`, `IDirtyRegion`
- 值对象：`AttentionLevel`, `DirtyRegion`, `InterestPoint`

---

### HashLife - 业务层

**职责**：HashLife 缓存的实现。

**依赖**：
- `Core` - 使用 `Timestamp` 等值对象
- `Spacetime` - 使用节点哈希

**被依赖**：无直接被依赖，通过缓存接口提供服务。

**核心类型**：
- 接口：`IHashLifeCache`, `IEvolutionResult`
- 值对象：`CacheEntry`, `EvolutionKey`

---

### Rules - 业务层

**职责**：规则系统的实现。

**依赖**：
- `Core` - 使用 `RuleType` 等枚举

**被依赖**：无直接被依赖，通过规则引擎接口提供服务。

**核心类型**：
- 接口：`IRule`, `IRuleEngine`, `IDeterministicRule`, `IEmergentRule`
- 值对象：`RulePriority`

---

### Rendering - 应用层

**职责**：渲染解耦层的实现。

**依赖**：
- `Core` - 使用 `EntityId`, `Timestamp` 等
- `Causal` - 使用因果特征向量
- `Collapse` - 使用坍缩场景

**被依赖**：无，这是最顶层模块。

**核心类型**：
- 接口：`ICollapsedScene`, `IRenderableEntity`
- 值对象：`CausalFeatures`, `SceneDescription`

---

### World - 应用层

**职责**：世界生成的实现。

**依赖**：
- `Core` - 使用 `RegionId` 等
- `Collapse` - 使用 WFC 生成

**被依赖**：无，这是最顶层模块。

**核心类型**：
- 接口：`IWorldGenerator`, `INoiseGenerator`, `IWFCGenerator`
- 值对象：`WorldSeed`
- 枚举：`BiomeType`

---

### Persistence - 应用层

**职责**：持久化层的实现。

**依赖**：
- `Core` - 使用 `RegionId` 等
- `Spacetime` - 存储节点状态

**被依赖**：无，这是最顶层模块。

**核心类型**：
- 接口：`IRepository<T>`, `IHistoryStore`, `IIncrementalSave`
- 实现：`InMemoryHistoryStore`

---

## 循环依赖检测

项目禁止循环依赖。CI 流程会自动检测循环依赖。

```bash
# 检测循环依赖的命令
dotnet list package --include-transitive
```

---

## 依赖注入

模块间通过接口进行依赖注入，避免直接实现依赖。

```csharp
// 正确：通过接口注入
public class CollapseService
{
    private readonly ICollapser _collapser;
    private readonly ICausalWeightSystem _weightSystem;
    
    public CollapseService(ICollapser collapser, ICausalWeightSystem weightSystem)
    {
        _collapser = collapser;
        _weightSystem = weightSystem;
    }
}

// 错误：直接依赖实现
public class CollapseService
{
    private readonly WaveFunctionCollapser _collapser;  // 不应直接依赖实现
}
```

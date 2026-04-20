# 测试指南

本文档介绍 Genesis 引擎项目的测试策略和最佳实践。

---

## 测试策略

### 测试金字塔

```
        ┌─────────┐
        │  E2E    │  端到端测试
        ├─────────┤
        │ 集成测试 │  模块间协作
    ┌───┴─────────┴───┐
    │    单元测试      │  独立功能验证
    └─────────────────┘
```

### 测试分类

| 类型 | 比例 | 关注点 |
|:---|:---|:---|
| 单元测试 | 70% | 单个类/方法的功能正确性 |
| 集成测试 | 20% | 模块间协作的正确性 |
| 端到端测试 | 10% | 完整场景的正确性 |

---

## 测试项目结构

```
projects/Genesis/tests/
├── Core.Tests/              # 核心模块测试
│   ├── ValueObjects/        # 值对象测试
│   └── Enums/               # 枚举测试
├── Spacetime.Tests/         # 时空模块测试
│   ├── SpacetimeNodeTests.cs
│   └── SpacetimeTreeTests.cs
├── Causal.Tests/            # 因果模块测试
├── Collapse.Tests/          # 坍缩模块测试
├── Attention.Tests/         # 注意力模块测试
├── HashLife.Tests/          # HashLife 模块测试
└── Integration.Tests/       # 集成测试
    ├── CollapsePipelineTests.cs
    └── AttentionCollapseTests.cs
```

---

## 单元测试示例

### 值对象测试

```csharp
using Genesis.Core.ValueObjects;

namespace Genesis.Core.Tests.ValueObjects;

public class PositionTests
{
    [Fact]
    public void DistanceTo_SamePosition_ReturnsZero()
    {
        var position = new Position(1.0, 2.0, 3.0);
        
        var distance = position.DistanceTo(position);
        
        Assert.Equal(0.0, distance);
    }
    
    [Theory]
    [InlineData(0, 0, 0, 3, 4, 0, 5)]
    [InlineData(0, 0, 0, 1, 1, 1, 1.732)]
    public void DistanceTo_TwoPositions_ReturnsCorrectDistance(
        double x1, double y1, double z1,
        double x2, double y2, double z2,
        double expected)
    {
        var p1 = new Position(x1, y1, z1);
        var p2 = new Position(x2, y2, z2);
        
        var distance = p1.DistanceTo(p2);
        
        Assert.Equal(expected, distance, 3);
    }
}
```

### 接口实现测试

```csharp
using Genesis.Spacetime.Interfaces;
using Genesis.Core.Enums;

namespace Genesis.Spacetime.Tests;

public class SpacetimeNodeTests
{
    [Fact]
    public void Level_WhenCreated_ReturnsCorrectLevel()
    {
        var node = CreateTestNode(NodeLevel.L2);
        
        Assert.Equal(NodeLevel.L2, node.Level);
    }
    
    [Fact]
    public void UpdateHistoryHash_WhenCalled_ChangesHash()
    {
        var node = CreateTestNode(NodeLevel.L0);
        var originalHash = node.HistoryHash;
        
        node.UpdateHistoryHash();
        
        Assert.NotEqual(originalHash, node.HistoryHash);
    }
    
    private static ISpacetimeNode CreateTestNode(NodeLevel level)
    {
        // 创建测试用节点
        return new TestSpacetimeNode(level);
    }
}
```

---

## 集成测试示例

### 坍缩管道测试

```csharp
using Genesis.Collapse.Interfaces;
using Genesis.Spacetime.Interfaces;

namespace Genesis.Integration.Tests;

public class CollapsePipelineTests
{
    private readonly ICollapser _collapser;
    private readonly IWaveFunctionState _state;
    
    public CollapsePipelineTests()
    {
        _collapser = CreateCollapser();
        _state = CreateTestState(100);
    }
    
    [Fact]
    public void Collapse_WithValidConstraints_CompletesSuccessfully()
    {
        var constraints = CreateTestConstraints();
        
        var result = _collapser.Collapse(_state, constraints);
        
        Assert.True(result);
        Assert.True(_collapser.IsFullyCollapsed(_state));
    }
    
    [Fact]
    public void Collapse_WithConflictingConstraints_ReturnsFalse()
    {
        var constraints = CreateConflictingConstraints();
        
        var result = _collapser.Collapse(_state, constraints);
        
        Assert.False(result);
    }
}
```

---

## 测试数据构建

### 测试数据构建器

```csharp
public class SpacetimeNodeBuilder
{
    private ulong _spatialHash = 12345;
    private NodeLevel _level = NodeLevel.L0;
    private Bounds _bounds = new(0, 0, 0, 100, 100, 100);
    
    public SpacetimeNodeBuilder WithSpatialHash(ulong hash)
    {
        _spatialHash = hash;
        return this;
    }
    
    public SpacetimeNodeBuilder WithLevel(NodeLevel level)
    {
        _level = level;
        return this;
    }
    
    public SpacetimeNodeBuilder WithBounds(Bounds bounds)
    {
        _bounds = bounds;
        return this;
    }
    
    public ISpacetimeNode Build()
    {
        return new TestSpacetimeNode(_spatialHash, _level, _bounds);
    }
    
    public static SpacetimeNodeBuilder Default() => new();
}
```

### 使用示例

```csharp
[Fact]
public void ProcessNode_WithHighLevelNode_SkipsCollapse()
{
    var node = SpacetimeNodeBuilder.Default()
        .WithLevel(NodeLevel.L3)
        .Build();
    
    var result = _processor.Process(node);
    
    Assert.False(result.Collapsed);
}
```

---

## 运行测试

### 运行所有测试

```bash
dotnet test
```

### 运行特定测试

```bash
# 运行特定项目
dotnet test projects/Genesis/tests/Core.Tests

# 运行特定类
dotnet test --filter "FullyQualifiedName~PositionTests"

# 运行特定方法
dotnet test --filter "FullyQualifiedName~PositionTests.DistanceTo_SamePosition_ReturnsZero"
```

### 生成覆盖率报告

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 测试最佳实践

1. **命名规范**：测试方法名应清晰描述测试场景
   - 格式：`MethodName_Scenario_ExpectedResult`
   
2. **单一断言**：每个测试只验证一个行为
   
3. **独立性**：测试之间不应有依赖关系
   
4. **可重复性**：测试结果应稳定可重复
   
5. **快速执行**：单元测试应在毫秒级完成

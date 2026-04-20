# Mod 开发指南

本指南介绍如何为 Genesis 引擎开发 Mod，包括规则热插拔、自定义层级行为和安全隔离。

## 规则热插拔

Genesis 支持运行时加载 Mod 引入的新规则。

### 规则类型

| 类型 | 特点 | 计算模式 |
|------|------|----------|
| 确定性规则 | 输入输出严格对应；可缓存 | HashLife 加速 |
| 涌现性规则 | 统计性、随机性；可近似 | WFC 坍缩 + 伪随机 |

### 定义规则

```csharp
using Genesis.Core.Enums;
using Genesis.Core.Interfaces;

[Rule(Priority = 100, Type = RuleType.Deterministic)]
public class SandPhysicsRule : IRule
{
    public int Priority => 100;
    public RuleType Type => RuleType.Deterministic;
    
    public void Execute(ChunkContext context, float deltaTime)
    {
        foreach (var cell in context.DirtyCells)
        {
            if (cell.Material == Material.Sand)
            {
                ApplyGravity(cell, context);
            }
        }
    }
    
    private void ApplyGravity(Cell cell, ChunkContext context)
    {
        var below = context.GetCell(cell.X, cell.Y - 1);
        if (below.IsEmpty)
        {
            context.Swap(cell, below);
        }
    }
}
```

### 注册规则

```csharp
public class MyMod : IMod
{
    public void OnLoad(IModRegistry registry)
    {
        registry.RegisterRule<SandPhysicsRule>();
        registry.RegisterRule<FireSpreadRule>();
    }
    
    public void OnUnload(IModRegistry registry)
    {
        registry.UnregisterRule<SandPhysicsRule>();
    }
}
```

## 自定义层级行为

Mod 可以为特定生物群系定义自定义层级更新逻辑。

### 层级更新接口

```csharp
public interface ILevelBehavior
{
    NodeLevel Level { get; }
    void OnCollapse(ISpacetimeNode node, CollapseContext ctx);
    void OnEvolve(ISpacetimeNode node, float deltaTime);
}
```

### 示例：森林火灾蔓延

```csharp
public class ForestFireBehavior : ILevelBehavior
{
    public NodeLevel Level => NodeLevel.L2;
    
    public void OnCollapse(ISpacetimeNode node, CollapseContext ctx)
    {
        var fireFront = ctx.GetMacroParameter("fire_front");
        var random = new DeterministicRandom(ctx.Seed);
        
        foreach (var cell in node.GetCells())
        {
            if (IsNearFireFront(cell, fireFront))
            {
                cell.State = random.NextDouble() < 0.7 
                    ? CellState.Burning 
                    : CellState.Normal;
            }
        }
    }
    
    public void OnEvolve(ISpacetimeNode node, float deltaTime)
    {
        var spreadRate = 0.1 * deltaTime;
        ctx.SetMacroParameter("fire_front", 
            AdvanceFireFront(spreadRate));
    }
}
```

### 注册层级行为

```csharp
public class ForestMod : IMod
{
    public void OnLoad(IModRegistry registry)
    {
        registry.RegisterLevelBehavior<Biome.Forest, ForestFireBehavior>();
    }
}
```

## 安全与隔离

用户 Mod 运行在沙箱环境中，确保引擎稳定性。

### 沙箱限制

| 限制项 | 说明 |
|--------|------|
| 文件访问 | 仅限 Mod 目录 |
| 网络访问 | 禁止 |
| 原生调用 | 禁止 |
| 历史哈希 | 只读，不可直接修改 |

### 安全 API

Mod 只能通过安全 API 访问世界状态：

```csharp
public interface IModContext
{
    IReadOnlyWorld World { get; }
    IRuleRegistry Rules { get; }
    void ScheduleUpdate(Position pos, float delay);
    void EmitEvent<T>(T evt) where T : IDomainEvent;
}
```

### 禁止操作示例

```csharp
public class BadMod : IMod
{
    public void OnLoad(IModContext ctx)
    {
        // ❌ 禁止：直接修改历史哈希
        // node.HistoryHash = 0; 
        
        // ❌ 禁止：访问文件系统
        // File.ReadAllText("/etc/passwd");
        
        // ✅ 允许：通过 API 提交规则
        ctx.Rules.RegisterRule<MyRule>();
    }
}
```

## Mod 结构示例

```
MyMod/
├── mod.json           # 元数据
├── MyMod.dll          # 编译后的程序集
├── assets/
│   ├── textures/
│   └── sounds/
└── config/
    └── settings.json
```

### mod.json 示例

```json
{
    "id": "com.example.mymod",
    "name": "My Mod",
    "version": "1.0.0",
    "entryPoint": "MyMod.MyMod",
    "dependencies": []
}
```

## 最佳实践

1. **优先级设计**：确定性规则优先级应高于涌现性规则
2. **缓存友好**：标记可缓存规则以利用 HashLife 加速
3. **因果自洽**：通过 `IModContext` 提交变更，避免破坏一致性
4. **性能测试**：在大型世界测试 Mod 性能影响

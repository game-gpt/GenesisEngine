# 编码规范

本文档定义 Genesis 引擎项目的编码规范。

---

## ⚠️ 语言分层原则（最高优先级）

> **违反此原则是致命架构错误，必须立即修正。**

Genesis 基于 Gnosis 元引擎构建，遵循严格的三层蛋糕模型。语言选择由层级决定，**不可逾越**：

| 层级 | 名称 | 编写语言 | 职责 |
|:---|:---|:---|:---|
| **Layer 1** | Gnosis 元引擎层 | C#（零外部依赖） | 25 个包构成的基础设施 |
| **Layer 2** | Genesis 游戏引擎层 | C#（调用 Gnosis 包） | 编辑器、资产管线、构建工具、启动器 |
| **Layer 3** | 游戏内容层 | **gg 语言族（GGScript / GGShader / GGWidget / GGNeural / GGObject）** | 游戏本体、Mod、DLC、插件、编辑器 Widget |

### 核心规则

1. **Layer 3 禁止使用 C#**：游戏内容（Terraria、Minecraft 等示例项目）必须 100% 使用 GGScript/GGShader 编写，**一行 C# 都不允许**。
2. **C# 仅限 Layer 1 和 Layer 2**：引擎基础设施、编辑器、构建管线等使用 C#，游戏开发者不需要也不应该直接使用 C#。
3. **游戏开发者接口通过 GGScript 原生函数绑定提供**：Genesis 不应直接暴露 C# API 给游戏开发者。

### 致命错误示例

- ❌ 在 `projects/Genesis/Terraria/` 中写 C# 游戏逻辑 → **游戏逻辑属于 Layer 3，必须用 GGScript 写在 `examples/Genesis.Terraria/` 中**
- ❌ 在 `examples/Genesis.Terraria/` 中写 C# 代码 → **Layer 3 一行 C# 都不允许**
- ❌ 在游戏适配文档中使用 C# 代码示例 → **Layer 3 内容必须用 GGScript 示例**

### 正确的项目路径

| 内容 | 正确路径 | 语言 |
|:---|:---|:---|
| Genesis 引擎核心 | `projects/Genesis/Core/` | C# |
| Genesis 引擎宿主 | `projects/GenesisEngine/` | C# |
| Terraria 游戏示例 | `examples/Genesis.Terraria/` | GGScript / GGShader / GGObject |
| Minecraft 游戏示例 | `examples/Genesis.Minecraft/` | GGScript / GGShader / GGObject |

详见 [Gnosis 项目介绍 - 引擎元语言与游戏对象语言](../../../../Gnosis.cs/documentation/zh-hans/overview/introduction.md#关键概念引擎元语言与游戏对象语言)。

---

## 命名约定

### 大小写规则

| 标识符类型 | 命名风格 | 示例 |
|:---|:---|:---|
| 类、结构、枚举 | PascalCase | `SpacetimeNode` |
| 接口 | PascalCase + I前缀 | `ISpacetimeNode` |
| 公共属性、方法 | PascalCase | `GetHistoryHash()` |
| 私有字段 | camelCase + _前缀 | `_nodeHash` |
| 局部变量、参数 | camelCase | `nodeHash` |
| 常量 | PascalCase | `MaxLevel` |

### 示例代码

```csharp
public class SpacetimeNode : ISpacetimeNode
{
    private readonly ulong _spatialHash;
    
    public ulong SpatialHash => _spatialHash;
    
    public NodeLevel Level { get; init; }
    
    public const int MaxChildren = 8;
    
    public SpacetimeNode(ulong spatialHash)
    {
        _spatialHash = spatialHash;
    }
    
    public void UpdateHistoryHash()
    {
        // 方法实现
    }
}
```

---

## 接口命名

接口必须以 `I` 前缀开头，后接 PascalCase 名称。

```csharp
// 正确
public interface ISpacetimeNode
{
    ulong SpatialHash { get; }
    NodeLevel Level { get; }
}

public interface ICollapser
{
    bool Collapse(IWaveFunctionState state);
}

// 错误
public interface SpacetimeNode { }      // 缺少 I 前缀
public interface IspacetimeNode { }     // S 应大写
```

---

## 命名空间约定

命名空间遵循 `Genesis.{ModuleName}.{SubDirectory}` 格式。

```csharp
// 核心模块
namespace Genesis.Core;
namespace Genesis.Core.ValueObjects;
namespace Genesis.Core.Enums;

// 时空模块
namespace Genesis.Spacetime;
namespace Genesis.Spacetime.Interfaces;
namespace Genesis.Spacetime.ValueObjects;

// 因果模块
namespace Genesis.Causal;
namespace Genesis.Causal.Interfaces;
namespace Genesis.Collapse;
namespace Genesis.Attention;
```

---

## 值对象规范

值对象使用 `record struct` 类型定义，实现不可变性和值语义。

```csharp
// 正确：使用 record struct
public readonly record struct Position(double X, double Y, double Z)
{
    public double DistanceTo(Position other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}

public readonly record struct EntityId(Guid Value)
{
    public static EntityId New() => new(Guid.NewGuid());
    public static readonly EntityId Empty = new(Guid.Empty);
}

// 错误：使用 class
public class Position  // 应使用 record struct
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}
```

---

## 可空引用类型

项目启用可空引用类型（Nullable Reference Types），必须正确处理空值。

```csharp
// 启用可空引用类型
// 在 .csproj 中：<Nullable>enable</Nullable>

public class WaveFunctionState
{
    // 可空属性
    public IConstraint? ActiveConstraint { get; set; }
    
    // 非空属性，必须初始化
    public int CellCount { get; init; } = 0;
    
    // 集合属性，初始化为空集合
    public List<ulong> CellMasks { get; init; } = [];
}

public ISpacetimeNode? FindNode(ulong spatialHash)
{
    // 返回可空类型
    return _nodes.TryGetValue(spatialHash, out var node) ? node : null;
}

public void ProcessNode(ulong spatialHash)
{
    var node = FindNode(spatialHash);
    
    // 空值检查
    if (node is null)
    {
        return;
    }
    
    // 使用空条件运算符
    var level = node?.Level;
}
```

---

## 代码注释

代码注释使用简体中文编写。

### 类和接口注释

```csharp
/// <summary>
/// 时空四维树节点接口，代表一个空间范围和时间尺度。
/// </summary>
public interface ISpacetimeNode
{
    /// <summary>
    /// 获取节点的空间哈希值。
    /// </summary>
    ulong SpatialHash { get; }
    
    /// <summary>
    /// 获取节点的历史哈希值，反映该区域从初始到当前时刻的所有状态变化。
    /// </summary>
    ulong HistoryHash { get; }
}
```

### 方法注释

```csharp
/// <summary>
/// 更新节点的历史哈希值。
/// </summary>
/// <exception cref="InvalidOperationException">节点已销毁时抛出</exception>
public void UpdateHistoryHash()
{
    ObjectDisposedException.ThrowIf(_disposed, this);
    // 实现逻辑
}
```

### 行内注释

```csharp
public void CalculateAttention(Position position)
{
    // 计算距离因子：距离越近，注意力越高
    var distanceFactor = 1.0 / (1.0 + position.DistanceTo(_playerPosition));
    
    // 计算视角因子：玩家正前方的区域注意力更高
    var viewDot = Vector3.Dot(_viewDirection, position - _playerPosition);
}
```

---

## 代码组织

### 文件结构

```csharp
// 1. using 语句
using System;
using System.Collections.Generic;

// 2. 命名空间
namespace Genesis.Spacetime;

// 3. 类定义
public class SpacetimeTree : ISpacetimeTree
{
    // 3.1 常量
    public const int MaxDepth = 16;
    
    // 3.2 静态字段
    private static readonly Dictionary<ulong, ISpacetimeNode> _cache = [];
    
    // 3.3 实例字段
    private readonly ICollapser _collapser;
    
    // 3.4 构造函数
    public SpacetimeTree(ICollapser collapser)
    {
        _collapser = collapser;
    }
    
    // 3.5 属性
    public ISpacetimeNode? Root { get; private set; }
    
    // 3.6 公共方法
    public ISpacetimeNode? GetNode(ulong spatialHash) { }
    
    // 3.7 私有方法
    private void ValidateNode(ISpacetimeNode node) { }
}
```

---

## 最佳实践

1. **单一职责**：每个类只负责一个功能
2. **依赖注入**：通过构造函数注入依赖
3. **不可变性**：优先使用只读属性和 init 访问器
4. **异常处理**：使用 `ObjectDisposedException.ThrowIf()` 等辅助方法
5. **值对象**：使用 `record struct` 定义不可变值类型

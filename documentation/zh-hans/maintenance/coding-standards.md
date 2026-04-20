# 编码规范

本文档定义 Genesis 引擎项目的 C# 编码规范。

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

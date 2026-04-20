# 快速入门

## 前置要求

在开始使用 Genesis 引擎之前，请确保您的开发环境满足以下要求：

- **.NET SDK**：8.0 或更高版本
- **IDE**：Visual Studio 2022、JetBrains Rider 或 VS Code（配合 C# 扩展）
- **操作系统**：Windows 10/11、macOS 12+ 或主流 Linux 发行版
- **内存**：建议 16GB 以上（用于大型世界开发测试）

## 项目结构

```
GenesisEngine/
├── projects/
│   └── Genesis/
│       ├── Core/              # 核心抽象与基础类型
│       │   ├── Enums/         # 枚举定义
│       │   ├── Events/        # 领域事件系统
│       │   ├── Interfaces/    # 核心接口
│       │   └── ValueObjects/  # 值对象
│       └── Spacetime/         # 时空节点系统
├── GenesisEngine.sln          # 解决方案文件
└── readme.md
```

### 核心模块说明

| 模块 | 说明 |
|------|------|
| `Core` | 引擎核心，包含基础类型、接口和枚举定义 |
| `Spacetime` | 时空节点系统，实现层级区块与哈希记忆 |

## 基本设置步骤

### 1. 克隆项目

```bash
git clone <repository-url>
cd GenesisEngine
```

### 2. 还原依赖

```bash
dotnet restore
```

### 3. 构建项目

```bash
dotnet build
```

### 4. 运行测试

```bash
dotnet test
```

## 核心概念速览

### 层级节点 (NodeLevel)

Genesis 引擎使用四级层级结构：

- **L0**：全精度确定性模拟，玩家当前视野
- **L1**：近场活跃区，降低更新频率
- **L2**：中场概率云，WFC 叠加态
- **L3**：远场宏观演化，纯函数式存储

### 坍缩状态 (CollapseState)

世界单元存在两种状态：

- **Superposition（叠加态）**：未观测时，持有概率分布
- **Collapsed（坍缩态）**：观测后，持有确定值

### 历史哈希

每个区块维护一个 `HistoryHash`，反映该区域从初始到当前时刻的所有状态变化。历史哈希支持：

- 快速时间跳跃（O(log Δt)）
- 多玩家状态同步
- 增量存档

## 下一步

- 阅读 [API 参考](./api-reference.md) 了解核心接口
- 查看 [Mod 开发指南](./mod-development.md) 学习规则扩展

# 快速开始

本文档帮助您快速搭建 Genesis 引擎开发环境。

---

## 环境要求

### 必需软件

| 软件 | 版本要求 | 说明 |
|:---|:---|:---|
| .NET SDK | 10.0 或更高 | 项目目标框架 |
| Git | 最新版 | 版本控制 |

### 推荐IDE

- **JetBrains Rider** - 推荐，功能强大
- **Visual Studio 2022** - 官方IDE
- **Visual Studio Code** - 轻量级选择

---

## 克隆仓库

```bash
git clone <repository-url>
cd GenesisEngine
```

---

## 构建项目

### 还原依赖

```bash
dotnet restore
```

### 编译项目

```bash
# Debug 模式
dotnet build

# Release 模式
dotnet build -c Release
```

---

## 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定项目测试
dotnet test projects/Genesis/tests

# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

---

## 项目结构

```
GenesisEngine/
├── .github/                    # GitHub 配置
│   └── workflows/              # CI/CD 工作流
│       └── dotnet-ci.yml       # .NET CI 配置
├── documentation/              # 文档目录
│   └── zh-hans/                # 简体中文文档
│       ├── core-systems/       # 核心系统文档
│       ├── development/        # 开发指南
│       ├── game-adaptation/    # 游戏适配
│       ├── maintenance/        # 维护者文档
│       ├── overview/           # 概览
│       └── technical/          # 技术文档
├── projects/                   # 项目目录
│   └── Genesis/                # 核心模块
│       ├── Chaos.csproj        # 项目文件
│       └── readme.md           # 模块文档
├── GenesisEngine.sln           # 解决方案文件
├── .editorconfig               # 编辑器配置
├── .gitignore                  # Git 忽略规则
└── License.md                  # 许可证
```

---

## 核心模块说明

Genesis 模块是引擎的核心实现，包含以下系统：

| 系统 | 说明 |
|:---|:---|
| 时空四维树 | 空间与时间的统一数据结构 |
| 因果锚点 | 玩家行为的权重贡献系统 |
| 波函数坍缩 | 概率云到确定性的转换 |
| 注意力调度 | 计算资源的智能分配 |
| HashLife 缓存 | 时间跳跃优化 |
| 规则系统 | 确定性与涌现性规则引擎 |

---

## 下一步

- 阅读 [编码规范](./coding-standards.md)
- 参考 [测试指南](./testing-guide.md)
- 查看 [模块文档](../../projects/Genesis/readme.md)
- 了解 [模块依赖](./architecture/module-dependencies.md)

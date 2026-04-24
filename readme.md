# Genesis Engine

Genesis Engine 是一个基于 Gnosis 元引擎构建的涌现叙事引擎，专注于**因果涌现**和**涌现叙事**的游戏体验。

## 核心特性

| 特性 | 说明 |
|:---|:---|
| **涌现叙事** | 基于因果锚点和波函数坍缩的叙事生成 |
| **注意力调度** | 动态计算玩家注意力，优化内容生成 |
| **因果引擎** | 追踪因果关系，支持时间旅行和因果重写 |
| **时空树** | 分层时空数据结构，支持无限世界 |

## 技术栈

- **引擎基础**: Gnosis 元引擎
- **脚本语言**: Valkyrie (`.script`)
- **着色器语言**: Valkyrie Shader (`.shader`)
- **配置格式**: Gon (`.gon`)
- **剧本语言**: Verse (`.story`)

## 快速开始

```bash
dotnet run --project projects/GenesisEngine
```

## 文档

- [开发指南](documentation/zh-hans/development/getting-started.md)
- [架构设计](documentation/zh-hans/maintenance/architecture/module-dependencies.md)
- [核心系统](documentation/zh-hans/core-systems/)

## 许可证

MPL-2.0

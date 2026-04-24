# 开发指南

## 环境搭建

### 1. 安装 .NET 11 SDK

```bash
# Windows
winget install Microsoft.DotNet.SDK.11

# macOS
brew install dotnet

# Linux
sudo apt-get install dotnet-sdk-11.0
```

### 2. 克隆并构建

```bash
git clone https://github.com/nyar-vm/GenesisEngine.git
cd GenesisEngine
dotnet build
```

## 项目结构

```
GenesisEngine/
├── projects/
│   ├── Genesis/              # 核心库
│   │   ├── Causal/           # 因果系统
│   │   ├── Collapse/         # 波函数坍缩
│   │   ├── Attention/        # 注意力调度
│   │   ├── Spacetime/        # 时空树
│   │   └── ...
│   └── GenesisEngine/        # 可执行项目
├── examples/                 # 示例游戏
│   ├── Genesis.Game.Minecraft/
│   └── Genesis.Game.Terraria/
└── documentation/            # 文档
```

## 创建新游戏

### 1. 初始化项目

```bash
mkdir my-genesis-game
cd my-genesis-game
```

### 2. 创建基础文件

**genesis.yaml** - 项目配置：

```yaml
name: "My Genesis Game"
version: "0.1.0"
engine: "GenesisEngine"

entry:
  script: "src/main.script"
  scene: "scenes/title.story"
```

**src/main.script** - Valkyrie 入口脚本：

```valkyrie
import genesis::core::*;

fn main() {
    // 初始化世界
    let world = World::new("My World");
    
    // 加载场景
    world.load_scene("scenes/title.story");
    
    // 运行游戏循环
    world.run();
}
```

**scenes/title.story** - Verse 标题场景：

```verse
@scene title
@background "assets/title_bg.png"

@character narrator
 narrator: 欢迎来到 Genesis Engine 的世界

@choice
 - 开始游戏 -> @scene intro
 - 加载存档 -> @action load_game
 - 退出 -> @action exit
```

### 3. 运行游戏

```bash
dotnet run --project ../GenesisEngine/projects/GenesisEngine -- --game .
```

## 核心 API

### 世界管理

```valkyrie
// 创建世界
let world = World::new("My World");

// 加载配置
world.load_config("config/world.gon");

// 注册系统
world.register_system(MovementSystem);
world.register_system(RenderSystem);

// 运行
world.run();
```

### 实体操作

```valkyrie
// 创建实体
let player = world.create_entity();
player.add_component(Player { health: 100, speed: 5 });
player.add_component(Transform { x: 0, y: 0, z: 0 });

// 查询实体
let query = world.query(Player, Transform);
for (player, transform) in query {
    // ...
}
```

### 因果系统

```valkyrie
// 创建因果锚点
let anchor = CausalAnchor::new("key_decision");
anchor.add_branch("save_village", CausalWeight::high());
anchor.add_branch("ignore_village", CausalWeight::low());

// 坍缩叙事
let outcome = world.collapse(anchor);
```

## 调试技巧

### 启用详细日志

```bash
dotnet run -- --log-level debug
```

### 使用开发工具

```bash
# 启动带调试 UI 的模式
dotnet run -- --dev-mode
```

### 性能分析

```bash
# 生成性能报告
dotnet run -- --profile --profile-output perf.json
```

## 最佳实践

1. **组件设计** - 保持组件小而专注
2. **系统分离** - 每个系统只处理一种逻辑
3. **配置驱动** - 使用 Gon 配置文件管理数据
4. **剧本分层** - 用 Verse 处理叙事，Valkyrie 处理逻辑

## 下一步

- [API 参考](api-reference.md)
- [模组开发](mod-development.md)
- [核心系统详解](../core-systems/)

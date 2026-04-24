# 快速开始

## 环境要求

- .NET 11 SDK
- Gnosis 元引擎（作为子模块或 NuGet 包）

## 创建第一个项目

```bash
# 克隆仓库
git clone https://github.com/nyar-vm/GenesisEngine.git
cd GenesisEngine

# 构建项目
dotnet build

# 运行示例
dotnet run --project projects/GenesisEngine
```

## 项目结构

```
my-genesis-game/
├── assets/
│   ├── textures/
│   ├── models/
│   └── sounds/
├── src/
│   ├── components.script      # Valkyrie 组件定义
│   ├── systems.script         # Valkyrie 系统逻辑
│   └── main.script            # 入口脚本
├── shaders/
│   ├── main.shader            # Valkyrie Shader 主着色器
│   └── postprocess.shader     # 后处理着色器
├── config/
│   ├── world.gon              # Gon 世界配置
│   └── entities.gon           # Gon 实体配置
├── stories/
│   └── intro.story            # Verse 剧本
└── genesis.yaml               # 项目配置
```

## 编写第一个 Valkyrie 脚本

创建 `src/main.script`：

```valkyrie
// 定义玩家组件
component Player {
    health: f32 = 100.0,
    speed: f32 = 5.0,
}

// 定义移动系统
system MovementSystem {
    query: (Player, Transform),
    
    on_update: (delta: f32) => {
        for (player, transform) in query {
            // 处理输入移动
        }
    }
}
```

## 编写第一个 Valkyrie Shader

创建 `shaders/main.shader`：

```valkyrie-shader
#version 450

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv;

layout(location = 0) out vec2 fragUv;

void main() {
    gl_Position = vec4(position, 1.0);
    fragUv = uv;
}
```

## 配置世界

创建 `config/world.gon`：

```gon
world {
    name: "My First World"
    seed: 12345
    
    entities: [
        {
            name: "Player"
            components: ["Player", "Transform", "Sprite"]
        }
    ]
}
```

## 运行游戏

```bash
dotnet run --project projects/GenesisEngine -- --game ./my-genesis-game
```

## 下一步

- [开发指南](../development/getting-started.md)
- [核心系统](../core-systems/)

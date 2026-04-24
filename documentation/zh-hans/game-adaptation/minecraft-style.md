# Minecraft 风格适配

本指南展示如何使用 Genesis Engine 创建 Minecraft 风格的游戏。

## 核心机制

| 机制 | 实现方式 |
|:---|:---|
| 方块世界 | 使用 `BlockComponent` 和 `ChunkSystem` |
| 挖掘/放置 | `MiningSystem` 处理交互 |
| 合成系统 | `CraftingSystem` 查询配方 |
| 生物群落 | `BiomeSystem` 基于噪声生成 |

## 项目结构

```
Genesis.Game.Minecraft/
├── components.script      # 方块、物品、生物组件
├── systems.script         # 挖掘、合成、生成系统
├── scenes/
│   └── game_main.story    # 主游戏场景
├── config/
│   ├── block_colors.gon   # 方块颜色配置
│   ├── crafting_recipes.gon # 合成配方
│   └── world.gon          # 世界生成配置
└── shaders/
    ├── block.shader       # 方块着色器
    └── camera.shader      # 相机着色器
```

## 关键组件

```valkyrie
// components.script
component Block {
    block_id: u32,
    durability: f32 = 1.0,
    is_solid: bool = true,
}

component Item {
    item_id: u32,
    count: u32 = 1,
    max_stack: u32 = 64,
}

component Player {
    inventory: [Item; 36],
    selected_slot: u32 = 0,
}
```

## 世界生成

```valkyrie
// systems.script
system WorldGenerationSystem {
    query: (Chunk, Transform),
    
    on_load: () => {
        for (chunk, transform) in query {
            generate_chunk(chunk, transform);
        }
    }
}

fn generate_chunk(chunk: Chunk, transform: Transform) {
    // 使用噪声生成地形
    let height = noise(transform.x, transform.z);
    
    for y in 0..height {
        if y < height - 3 {
            chunk.set_block(x, y, z, BlockId::Stone);
        } else if y < height - 1 {
            chunk.set_block(x, y, z, BlockId::Dirt);
        } else {
            chunk.set_block(x, y, z, BlockId::Grass);
        }
    }
}
```

## 配置文件

```gon
// config/world.gon
world {
    name: "Minecraft World"
    seed: 12345
    
    generation {
        chunk_size: 16
        max_height: 256
        sea_level: 63
    }
    
    biomes: [
        { name: "Plains", weight: 0.4 },
        { name: "Forest", weight: 0.3 },
        { name: "Desert", weight: 0.2 },
        { name: "Mountains", weight: 0.1 }
    ]
}

// config/crafting_recipes.gon
recipes {
    wooden_planks: {
        input: [{ item: "log", count: 1 }],
        output: { item: "planks", count: 4 }
    }
    
    crafting_table: {
        input: [{ item: "planks", count: 4 }],
        output: { item: "crafting_table", count: 1 }
    }
}
```

## 着色器

```valkyrie-shader
// shaders/block.shader
#version 450

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec3 normal;

layout(location = 0) out vec2 fragUv;
layout(location = 1) out vec3 fragNormal;

layout(set = 0, binding = 0) uniform Camera {
    mat4 view_proj;
    vec3 position;
} camera;

void main() {
    gl_Position = camera.view_proj * vec4(position, 1.0);
    fragUv = uv;
    fragNormal = normal;
}
```

## 运行

```bash
dotnet run --project projects/GenesisEngine -- --game examples/Genesis.Game.Minecraft
```

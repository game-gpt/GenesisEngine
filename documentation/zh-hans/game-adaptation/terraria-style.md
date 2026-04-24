# Terraria 风格适配

本指南展示如何使用 Genesis Engine 创建 Terraria 风格的 2D 沙盒游戏。

## 核心机制

| 机制 | 实现方式 |
|:---|:---|
| 2D 方块世界 | `TileComponent` 和 `WorldSystem` |
| 挖掘/建造 | `MiningSystem` 处理方块交互 |
| 物品/装备 | `InventorySystem` 管理物品 |
| 生物群落 | `BiomeSystem` 基于深度和位置 |
| Boss 战 | `BossSystem` 处理特殊敌人 |

## 项目结构

```
Genesis.Game.Terraria/
├── components.script      # 图块、物品、NPC 组件
├── systems.script         # 挖掘、生成、战斗系统
├── scenes/
│   └── game_main.story    # 主游戏场景
├── config/
│   ├── tile_colors.gon    # 图块颜色配置
│   └── world.gon          # 世界生成配置
└── shaders/
    ├── tile.shader        # 图块着色器
    ├── entity.shader      # 实体着色器
    └── renderer.shader    # 渲染器着色器
```

## 关键组件

```valkyrie
// components.script
component Tile {
    tile_id: u32,
    wall_id: u32 = 0,
    liquid: f32 = 0.0,
    is_active: bool = true,
}

component Item {
    item_id: u32,
    count: u32 = 1,
    prefix: u32 = 0,
}

component NPC {
    npc_id: u32,
    ai_style: u32 = 0,
    is_friendly: bool = false,
    is_boss: bool = false,
}
```

## 世界生成

```valkyrie
// systems.script
system WorldGenerationSystem {
    query: (World, Transform),
    
    on_load: () => {
        for (world, transform) in query {
            generate_surface(world);
            generate_underground(world);
            generate_caverns(world);
            generate_hell(world);
        }
    }
}

fn generate_surface(world: World) {
    // 生成地表地形
    for x in 0..world.width {
        let height = surface_height(x);
        
        for y in height..height + 5 {
            world.set_tile(x, y, TileId::Dirt);
        }
        
        world.set_tile(x, height - 1, TileId::Grass);
    }
}
```

## 配置文件

```gon
// config/world.gon
world {
    name: "Terraria World"
    seed: 12345
    
    size {
        width: 4200
        height: 1200
    }
    
    layers {
        surface: { min_y: 0, max_y: 200 }
        underground: { min_y: 200, max_y: 400 }
        caverns: { min_y: 400, max_y: 800 }
        underworld: { min_y: 800, max_y: 1200 }
    }
}
```

## 运行

```bash
dotnet run --project projects/GenesisEngine -- --game examples/Genesis.Game.Terraria
```

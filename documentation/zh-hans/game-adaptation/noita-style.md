# Noita 风格适配

本指南展示如何使用 Genesis Engine 创建 Noita 风格的像素物理沙盒游戏。

## 核心机制

| 机制 | 实现方式 |
|:---|:---|
| 像素物理 | `PixelPhysicsSystem` 处理每个像素 |
| 材料系统 | `MaterialComponent` 定义材料属性 |
| 法术系统 | `SpellSystem` 处理魔法效果 |
| 程序生成 | `ProceduralSystem` 生成世界 |

## 项目结构

```
Genesis.Game.Noita/
├── components.script      # 材料、法术、实体组件
├── systems.script         # 物理、法术、生成系统
├── scenes/
│   └── game_main.story    # 主游戏场景
├── config/
│   ├── materials.gon      # 材料属性配置
│   └── spells.gon         # 法术配置
└── shaders/
    ├── pixel.shader       # 像素着色器
    └── postprocess.shader # 后处理着色器
```

## 关键组件

```valkyrie
// components.script
component Material {
    material_id: u32,
    density: f32,
    viscosity: f32 = 0.0,
    is_liquid: bool = false,
    is_gas: bool = false,
    is_solid: bool = true,
    flammability: f32 = 0.0,
    corrosion: f32 = 0.0,
}

component Spell {
    spell_id: u32,
    mana_cost: f32,
    cooldown: f32,
    charges: i32 = -1,
}

component Wand {
    capacity: u32,
    spells: [Spell; 10],
    shuffle: bool = false,
}
```

## 像素物理系统

```valkyrie
// systems.script
system PixelPhysicsSystem {
    query: (World, Transform),
    
    on_update: (delta: f32) => {
        for (world, transform) in query {
            // 更新每个像素
            for x in 0..world.width {
                for y in 0..world.height {
                    update_pixel(world, x, y);
                }
            }
        }
    }
}

fn update_pixel(world: World, x: u32, y: u32) {
    let pixel = world.get_pixel(x, y);
    
    if (pixel.material.is_liquid) {
        // 液体流动
        if (world.is_empty(x, y + 1)) {
            world.move_pixel(x, y, x, y + 1);
        } else if (world.is_empty(x - 1, y + 1)) {
            world.move_pixel(x, y, x - 1, y + 1);
        } else if (world.is_empty(x + 1, y + 1)) {
            world.move_pixel(x, y, x + 1, y + 1);
        }
    } else if (pixel.material.is_gas) {
        // 气体上升
        if (world.is_empty(x, y - 1)) {
            world.move_pixel(x, y, x, y - 1);
        }
    }
}
```

## 配置文件

```gon
// config/materials.gon
materials {
    sand: {
        density: 1.5,
        is_solid: true,
        color: [194, 178, 128]
    }
    
    water: {
        density: 1.0,
        is_liquid: true,
        viscosity: 0.1,
        color: [28, 107, 160]
    }
    
    oil: {
        density: 0.8,
        is_liquid: true,
        viscosity: 0.5,
        flammability: 1.0,
        color: [80, 60, 40]
    }
    
    fire: {
        density: 0.1,
        is_gas: true,
        flammability: 1.0,
        color: [255, 100, 0]
    }
}

// config/spells.gon
spells {
    spark_bolt: {
        mana_cost: 5.0,
        damage: 3.0,
        speed: 100.0,
        lifetime: 2.0
    }
    
    bomb: {
        mana_cost: 25.0,
        damage: 50.0,
        radius: 5.0,
        explosive: true
    }
    
    teleport: {
        mana_cost: 15.0,
        range: 50.0,
        cooldown: 3.0
    }
}
```

## 运行

```bash
dotnet run --project projects/GenesisEngine -- --game examples/Genesis.Game.Noita
```

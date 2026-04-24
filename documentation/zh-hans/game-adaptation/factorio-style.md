# Factorio 风格适配

本指南展示如何使用 Genesis Engine 创建 Factorio 风格的自动化工厂游戏。

## 核心机制

| 机制 | 实现方式 |
|:---|:---|
| 传送带系统 | `BeltSystem` 处理物品传输 |
| 机器合成 | `CraftingSystem` 处理配方 |
| 物流网络 | `LogisticSystem` 管理机器人 |
| 资源生成 | `ResourceSystem` 生成矿脉 |

## 项目结构

```
Genesis.Game.Factorio/
├── components.script      # 机器、传送带、物品组件
├── systems.script         # 自动化、物流、生产系统
├── scenes/
│   └── game_main.story    # 主游戏场景
├── config/
│   ├── recipes.gon        # 生产配方
│   └── world.gon          # 世界生成配置
└── shaders/
    ├── entity.shader      # 实体着色器
    └── belt.shader        # 传送带着色器
```

## 关键组件

```valkyrie
// components.script
component Machine {
    recipe_id: u32,
    progress: f32 = 0.0,
    input_inventory: [Item; 4],
    output_inventory: [Item; 2],
}

component Belt {
    direction: Vec2,
    speed: f32 = 1.0,
    items: [Item; 8],
}

component Power {
    consumption: f32,
    production: f32 = 0.0,
    buffer: f32 = 0.0,
    max_buffer: f32 = 1000.0,
}
```

## 自动化系统

```valkyrie
// systems.script
system ProductionSystem {
    query: (Machine, Power),
    
    on_update: (delta: f32) => {
        for (machine, power) in query {
            if (power.buffer < machine.power_consumption) {
                continue;
            }
            
            // 检查输入
            if (!has_required_inputs(machine)) {
                continue;
            }
            
            // 生产进度
            machine.progress += delta;
            
            if (machine.progress >= 1.0) {
                produce_output(machine);
                machine.progress = 0.0;
            }
        }
    }
}
```

## 配置文件

```gon
// config/recipes.gon
recipes {
    iron_plate: {
        time: 3.2,
        input: [{ item: "iron_ore", count: 1 }],
        output: [{ item: "iron_plate", count: 1 }]
    }
    
    copper_cable: {
        time: 0.5,
        input: [{ item: "copper_plate", count: 1 }],
        output: [{ item: "copper_cable", count: 2 }]
    }
    
    electronic_circuit: {
        time: 0.5,
        input: [
            { item: "iron_plate", count: 1 },
            { item: "copper_cable", count: 3 }
        ],
        output: [{ item: "electronic_circuit", count: 1 }]
    }
}
```

## 运行

```bash
dotnet run --project projects/GenesisEngine -- --game examples/Genesis.Game.Factorio
```

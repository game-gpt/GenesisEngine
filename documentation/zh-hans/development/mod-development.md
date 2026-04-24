# 模组开发指南

Genesis Engine 支持丰富的模组系统，允许第三方开发者扩展游戏内容。

## 模组结构

```
my-mod/
├── mod.yaml              # 模组元数据
├── src/
│   ├── components.script  # Valkyrie 组件
│   ├── systems.script     # Valkyrie 系统
│   └── events.script      # Valkyrie 事件处理
├── assets/
│   ├── textures/
│   ├── models/
│   └── sounds/
├── config/
│   └── entities.gon       # Gon 实体配置
└── stories/
    └── new_chapter.story  # Verse 剧本
```

## mod.yaml

```yaml
id: "my-mod"
name: "我的模组"
version: "1.0.0"
author: "Your Name"
description: "这是一个示例模组"

dependencies:
  - id: "base-game"
    version: ">=0.1.0"

entry:
  script: "src/main.script"
```

## 创建组件

```valkyrie
// src/components.script
component ModComponent {
    custom_value: f32 = 0.0,
    enabled: bool = true,
}
```

## 创建系统

```valkyrie
// src/systems.script
system ModSystem {
    query: (ModComponent, Transform),
    
    on_update: (delta: f32) => {
        for (mod, transform) in query {
            if (mod.enabled) {
                transform.x += mod.custom_value * delta;
            }
        }
    }
}
```

## 添加剧本

```verse
// stories/new_chapter.story
@scene new_chapter
@background "assets/new_bg.png"

@character guide
 guide: 欢迎体验模组内容！

@action unlock_achievement("mod_explorer")
```

## 加载模组

```bash
# 通过命令行加载
dotnet run -- --mods ./my-mod

# 或通过配置文件
genesis.yaml:
  mods:
    - path: "./my-mod"
      enabled: true
```

## 最佳实践

1. **命名空间** - 使用模组 ID 作为前缀避免冲突
2. **依赖管理** - 明确声明依赖版本
3. **兼容性** - 测试与不同游戏版本的兼容性
4. **文档** - 提供清晰的安装和使用说明

## 发布模组

1. 打包为 `.zip` 文件
2. 上传到模组平台
3. 提供版本更新日志

## 示例模组

参考 `examples/` 目录下的示例模组：

- `Genesis.Game.Minecraft/` - Minecraft 风格模组
- `Genesis.Game.Terraria/` - Terraria 风格模组

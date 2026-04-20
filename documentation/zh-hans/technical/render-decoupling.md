# 渲染与模拟解耦

## 概述

Genesis 引擎采用渲染与模拟完全解耦的架构。模拟层（因果层）负责世界状态的因果演化，输出确定性场景描述；渲染层作为可插拔后端，独立负责视觉表现。

## CollapsedScene 接口

### 接口定义

```python
class CollapsedScene:
    entities: List[Entity]
    causal_features: Vector[float]

class Entity:
    type: str
    transform: Transform
    causal_features: Vector[float]
    properties: Dict[str, Any]
```

### 字段说明

| 字段 | 类型 | 说明 |
|------|------|------|
| `entities` | `List[Entity]` | 坍缩后的确定性实体列表 |
| `causal_features` | `Vector[float]` | 全局因果特征向量 |
| `type` | `str` | 实体类型标识 |
| `transform` | `Transform` | 空间变换 |
| `properties` | `Dict[str, Any]` | 扩展属性（破损度、发光强度等） |

### 因果特征向量

因果特征向量是低维连续向量（通常 < 16 维），总结实体的因果历史：

| 维度 | 示例含义 |
|------|----------|
| 0 | 希望值 |
| 1 | 智慧值 |
| 2 | 混乱值 |
| 3 | 时间侵蚀度 |
| ... | 可扩展 |

## 可插拔渲染后端

### 渲染函数契约

渲染层只需实现：

```python
def render(scene: CollapsedScene) -> Frame:
    pass
```

### 后端方案对比

| 方案 | 描述 | 适用场景 |
|------|------|----------|
| **传统光栅化** | 预置网格、材质、光照 | 性能敏感、兼容现有资产 |
| **NeRF / 3D Gaussian** | 因果特征作为条件输入 | 无限细节、高保真 |
| **端到端扩散模型** | 场景描述直接生成帧 | AI 原生游戏、极致画质 |
| **混合渲染** | 远 NeRF 近光栅化 | 性能与质量平衡 |

## AI 渲染集成

### 集成优势

Genesis 因果层是 AI 渲染引擎的理想前端：

| 优势 | 说明 |
|------|------|
| 紧凑输入 | 确定性场景描述降低扩散模型条件维度 |
| 帧间一致 | 因果哈希链保证时序稳定 |
| 注意力引导 | 高关注区域消耗更多算力 |

### 集成架构

```
┌─────────────────┐
│   因果层        │
│ (CollapsedScene)│
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  渲染后端接口   │
└────────┬────────┘
         │
    ┌────┴────┬────────────┐
    ▼         ▼            ▼
┌───────┐ ┌───────┐  ┌──────────┐
│光栅化 │ │ NeRF  │  │ 扩散模型 │
└───────┘ └───────┘  └──────────┘
```

### 扩散模型示例

```python
def ai_render(scene: CollapsedScene, prev_frame: Frame) -> Frame:
    condition = encode_scene(scene)
    condition += encode_causal_features(scene.causal_features)
    condition += encode_temporal(prev_frame)
    return diffusion_model.generate(condition)
```

## 性能考量

### 数据流分离

| 层级 | 模拟层访问 | 渲染层访问 |
|------|------------|------------|
| L0 | 读写 | 只读 |
| L1 | 读写 | 只读 |
| L2+ | 读写 | 不直接访问 |

### 增量更新

渲染层采用增量网格更新：

```csharp
void UpdateMesh(HChunkNode chunk) {
    if (chunk.IsHomogeneous) {
        UseMergedMesh(chunk);
    } else {
        RebuildDirtyRegions(chunk);
    }
}
```

### LOD 策略

| 距离 | 渲染策略 |
|------|----------|
| 近距离 | 全精度网格 |
| 中距离 | 简化网格 + 层级纹理 |
| 远距离 | Impostor / 低模 LOD |

## 职责边界

### 模拟层负责

- ✅ 世界状态的概率表示
- ✅ 因果锚点累积与传播
- ✅ 注意力驱动的坍缩决策
- ✅ 因果哈希链维护

### 渲染层负责

- ✅ 像素渲染（颜色、深度、法线）
- ✅ 几何网格生成
- ✅ 材质与纹理
- ✅ 光照计算
- ✅ 帧输出

### 明确不做

模拟层**不负责**：
- ❌ 物理碰撞精确解
- ❌ 音频合成
- ❌ 网络传输细节

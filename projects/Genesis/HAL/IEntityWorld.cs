using Genesis.Core;

namespace Genesis.HAL;

/// <summary>
/// 实体世界抽象接口
/// 内核通过此接口查询实体，不直接依赖 Gnosis.ECS.World.World
/// HAL 实现负责将调用委托给 Gnosis.ECS.World.World
/// </summary>
public interface IEntityWorld
{
    /// <summary>
    /// 创建实体查询构建器
    /// </summary>
    IEntityQueryBuilder CreateQuery();

    /// <summary>
    /// 检查实体是否拥有指定类型的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="entityId">实体标识</param>
    /// <returns>是否拥有组件</returns>
    bool HasComponent<T>(EntityId entityId) where T : struct;

    /// <summary>
    /// 获取实体的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="entityId">实体标识</param>
    /// <returns>组件实例</returns>
    T GetComponent<T>(EntityId entityId) where T : struct;

    /// <summary>
    /// 设置实体的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="entityId">实体标识</param>
    /// <param name="component">组件实例</param>
    void SetComponent<T>(EntityId entityId, T component) where T : struct;

    /// <summary>
    /// 帧更新
    /// </summary>
    /// <param name="delta">帧间隔时间（秒）</param>
    void Update(float delta);
}

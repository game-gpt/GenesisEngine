using Genesis.Core;

namespace Genesis.HAL;

/// <summary>
/// 实体查询构建器接口
/// 抽象 Gnosis.ECS.World.World.CreateQuery() 的查询能力
/// </summary>
public interface IEntityQueryBuilder
{
    /// <summary>
    /// 要求实体拥有指定类型的组件（All 过滤）
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <returns>构建器自身（链式调用）</returns>
    IEntityQueryBuilder All<T>() where T : struct;

    /// <summary>
    /// 构建查询并返回匹配的实体列表
    /// </summary>
    /// <returns>匹配的实体标识列表</returns>
    IReadOnlyList<EntityId> Build();
}

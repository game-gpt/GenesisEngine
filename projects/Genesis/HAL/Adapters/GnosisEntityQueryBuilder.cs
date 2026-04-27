using Genesis.Core;
using GnosisEntityId = Gnosis.Core.Entity.EntityId;

namespace Genesis.HAL.Adapters;

/// <summary>
/// IEntityQueryBuilder 的 Gnosis 查询构建器适配器
/// </summary>
public sealed class GnosisEntityQueryBuilder : IEntityQueryBuilder
{
    #region 字段

    private readonly Gnosis.ECS.World.QueryBuilder _queryBuilder;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 Gnosis 实体查询构建器适配器
    /// </summary>
    /// <param name="queryBuilder">Gnosis 查询构建器</param>
    public GnosisEntityQueryBuilder(Gnosis.ECS.World.QueryBuilder queryBuilder)
    {
        _queryBuilder = queryBuilder;
    }

    #endregion

    #region IEntityQueryBuilder 实现

    /// <summary>
    /// 要求实体拥有指定类型的组件
    /// </summary>
    public IEntityQueryBuilder All<T>() where T : struct
    {
        _queryBuilder.All<T>();
        return this;
    }

    /// <summary>
    /// 构建查询并返回匹配的实体列表
    /// </summary>
    public IReadOnlyList<EntityId> Build()
    {
        var gnosisIds = _queryBuilder.Build();
        return gnosisIds.Select(id => (EntityId)id).ToList();
    }

    #endregion
}

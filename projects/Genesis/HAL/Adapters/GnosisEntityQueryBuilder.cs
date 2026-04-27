using Genesis.Core;
using GnosisEntityId = Gnosis.Core.EntityId;

namespace Genesis.HAL.Adapters;

/// <summary>
/// IEntityQueryBuilder 的 Gnosis IQuery 适配器
/// </summary>
public sealed class GnosisEntityQueryBuilder : IEntityQueryBuilder
{
    #region 字段

    private readonly Gnosis.ECS.Query.IQuery _query;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 Gnosis 实体查询构建器适配器
    /// </summary>
    /// <param name="query">Gnosis IQuery 实例</param>
    public GnosisEntityQueryBuilder(Gnosis.ECS.Query.IQuery query)
    {
        _query = query;
    }

    #endregion

    #region IEntityQueryBuilder 实现

    /// <summary>
    /// 要求实体拥有指定类型的组件
    /// </summary>
    public IEntityQueryBuilder All<T>() where T : struct
    {
        _query.All<T>();
        return this;
    }

    /// <summary>
    /// 构建查询并返回匹配的实体列表
    /// </summary>
    public IReadOnlyList<EntityId> Build()
    {
        var gnosisIds = _query.Build();
        return gnosisIds.Select(id => (EntityId)id).ToList();
    }

    #endregion
}

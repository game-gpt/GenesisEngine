namespace Genesis.Core;

/// <summary>
/// 实体标识符，包含 32 位索引和 32 位代际，总计 64 位。
/// 代际用于检测已销毁实体的旧引用，防止访问错误实体。
/// 与 Gnosis.Core.Entity.EntityId 结构兼容，可隐式双向转换。
/// </summary>
public readonly record struct EntityId(uint Index, uint Generation)
{
    #region 静态字段

    private static int _nextIndex = 1;

    /// <summary>
    /// 空实体 ID，索引和代际均为 0
    /// </summary>
    public static readonly EntityId Null = new(0, 0);

    #endregion

    #region 属性

    /// <summary>
    /// 是否为空实体 ID
    /// </summary>
    public bool IsNull => Index == 0 && Generation == 0;

    #endregion

    #region 工厂方法

    /// <summary>
    /// 创建新的实体 ID，自动分配递增索引
    /// </summary>
    /// <returns>新实体 ID</returns>
    public static EntityId New()
    {
        var index = (uint)Interlocked.Increment(ref _nextIndex);
        return new EntityId(index, 1);
    }

    #endregion

    #region Gnosis 兼容转换

    /// <summary>
    /// 从 Gnosis EntityId 隐式转换为 Genesis EntityId
    /// </summary>
    /// <param name="gnosisId">Gnosis 实体 ID</param>
    /// <returns>Genesis 实体 ID</returns>
    public static implicit operator EntityId(Gnosis.Core.Entity.EntityId gnosisId)
    {
        return new EntityId(gnosisId.Index, gnosisId.Generation);
    }

    /// <summary>
    /// 从 Genesis EntityId 隐式转换为 Gnosis EntityId
    /// </summary>
    /// <param name="id">Genesis 实体 ID</param>
    /// <returns>Gnosis 实体 ID</returns>
    public static implicit operator Gnosis.Core.Entity.EntityId(EntityId id)
    {
        return new Gnosis.Core.Entity.EntityId(id.Index, id.Generation);
    }

    #endregion

    #region 重写

    public override string ToString()
    {
        return $"Entity({Index}:{Generation})";
    }

    #endregion
}

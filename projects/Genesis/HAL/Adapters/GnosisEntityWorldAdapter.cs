using Genesis.Core;
using GnosisEntityId = Gnosis.Core.EntityId;
using GnosisWorld = Gnosis.ECS.World.World;

namespace Genesis.HAL.Adapters;

/// <summary>
/// IEntityWorld 的 Gnosis.ECS.World.World 适配器
/// </summary>
public sealed class GnosisEntityWorldAdapter : IEntityWorld
{
    #region 字段

    private readonly GnosisWorld _world;

    #endregion

    #region 属性

    /// <summary>
    /// 底层 Gnosis World 实例
    /// </summary>
    public GnosisWorld GnosisWorld => _world;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 Gnosis 实体世界适配器
    /// </summary>
    /// <param name="world">Gnosis World 实例</param>
    public GnosisEntityWorldAdapter(GnosisWorld world)
    {
        ArgumentNullException.ThrowIfNull(world);
        _world = world;
    }

    #endregion

    #region IEntityWorld 实现

    /// <summary>
    /// 创建实体查询构建器
    /// </summary>
    public IEntityQueryBuilder CreateQuery()
    {
        return new GnosisEntityQueryBuilder(_world.CreateQuery());
    }

    /// <summary>
    /// 检查实体是否拥有指定类型的组件
    /// </summary>
    public bool HasComponent<T>(EntityId entityId) where T : struct
    {
        return _world.HasComponent<T>((GnosisEntityId)entityId);
    }

    /// <summary>
    /// 获取实体的组件
    /// </summary>
    public T GetComponent<T>(EntityId entityId) where T : struct
    {
        return _world.GetComponent<T>((GnosisEntityId)entityId);
    }

    /// <summary>
    /// 设置实体的组件
    /// </summary>
    public void SetComponent<T>(EntityId entityId, T component) where T : struct
    {
        _world.SetComponent((GnosisEntityId)entityId, component);
    }

    /// <summary>
    /// 帧更新
    /// </summary>
    public void Update(float delta)
    {
        _world.Update(delta);
    }

    #endregion
}

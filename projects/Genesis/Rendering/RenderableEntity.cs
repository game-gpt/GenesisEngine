using Genesis.Core;
using Genesis.Spacetime;

namespace Genesis.Rendering;

/// <summary>
/// 可渲染实体
/// </summary>
public sealed class RenderableEntity : IRenderableEntity
{
    #region 属性

    /// <summary>
    /// 实体标识
    /// </summary>
    public EntityId Id { get; }

    /// <summary>
    /// 实体类型
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// 位置
    /// </summary>
    public Position Position { get; }

    /// <summary>
    /// 因果特征
    /// </summary>
    public CausalFeatures Features { get; }

    /// <summary>
    /// 属性字典
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties { get; }

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化可渲染实体
    /// </summary>
    public RenderableEntity(
        EntityId id,
        string type,
        Position position,
        CausalFeatures features,
        IReadOnlyDictionary<string, object>? properties = null)
    {
        Id = id;
        Type = type;
        Position = position;
        Features = features;
        Properties = properties ?? new Dictionary<string, object>();
    }

    #endregion
}

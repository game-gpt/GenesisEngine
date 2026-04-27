using Genesis.Core;

namespace Genesis.Rendering;

/// <summary>
/// 可渲染实体接口
/// </summary>
public interface IRenderableEntity
{
    /// <summary>
    /// 实体标识
    /// </summary>
    EntityId Id { get; }

    /// <summary>
    /// 实体类型
    /// </summary>
    string Type { get; }

    /// <summary>
    /// 位置
    /// </summary>
    Position Position { get; }

    /// <summary>
    /// 因果特征
    /// </summary>
    CausalFeatures Features { get; }

    /// <summary>
    /// 属性字典
    /// </summary>
    IReadOnlyDictionary<string, object> Properties { get; }
}

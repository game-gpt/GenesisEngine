using Genesis.Core;
using Genesis.Spacetime;
using Gnosis.Core.Entity;
using Gnosis.ECS.World;

namespace Genesis.Rendering;

public sealed class RenderableEntity : IRenderableEntity
{
    #region 属性

    public EntityId Id { get; }
    public string Type { get; }
    public Position Position { get; }
    public CausalFeatures Features { get; }
    public IReadOnlyDictionary<string, object> Properties { get; }

    #endregion

    #region 构造函数

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

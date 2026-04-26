using Genesis.Core;
using Genesis.Spacetime;
using Gnosis.Core.Entity;

namespace Genesis.Rendering;

public sealed class CollapsedScene : ICollapsedScene
{
    #region 字段

    private readonly List<IRenderableEntity> _entities;

    #endregion

    #region 属性

    public IReadOnlyList<IRenderableEntity> Entities => _entities.AsReadOnly();
    public CausalFeatures GlobalFeatures { get; }
    public Timestamp GeneratedAt { get; }
    public ulong SceneHash { get; }

    #endregion

    #region 构造函数

    public CollapsedScene(
        IReadOnlyList<IRenderableEntity> entities,
        CausalFeatures globalFeatures,
        Timestamp generatedAt,
        ulong sceneHash)
    {
        _entities = new List<IRenderableEntity>(entities);
        GlobalFeatures = globalFeatures;
        GeneratedAt = generatedAt;
        SceneHash = sceneHash;
    }

    #endregion

    #region 公开方法

    public SceneDescription ToDescription()
    {
        if (_entities.Count == 0)
        {
            return new SceneDescription(0, GeneratedAt, 0, new Bounds(0, 0, 0, 0, 0, 0));
        }

        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var minZ = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;
        var maxZ = double.MinValue;

        foreach (var entity in _entities)
        {
            var pos = entity.Position;
            minX = Math.Min(minX, pos.X);
            minY = Math.Min(minY, pos.Y);
            minZ = Math.Min(minZ, pos.Z);
            maxX = Math.Max(maxX, pos.X);
            maxY = Math.Max(maxY, pos.Y);
            maxZ = Math.Max(maxZ, pos.Z);
        }

        var bounds = new Bounds(minX, minY, minZ, maxX, maxY, maxZ);
        return new SceneDescription(SceneHash, GeneratedAt, _entities.Count, bounds);
    }

    #endregion
}

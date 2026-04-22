using Genesis.Core.ValueObjects;
using Genesis.Rendering.ValueObjects;

namespace Genesis.Rendering.Interfaces;

public interface ICollapsedScene
{
    IReadOnlyList<IRenderableEntity> Entities { get; }
    CausalFeatures GlobalFeatures { get; }
    Timestamp GeneratedAt { get; }
    ulong SceneHash { get; }
}

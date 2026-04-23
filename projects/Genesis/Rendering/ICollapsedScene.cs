using Genesis.Core;

namespace Genesis.Rendering;

public interface ICollapsedScene
{
    IReadOnlyList<IRenderableEntity> Entities { get; }
    CausalFeatures GlobalFeatures { get; }
    Timestamp GeneratedAt { get; }
    ulong SceneHash { get; }
}

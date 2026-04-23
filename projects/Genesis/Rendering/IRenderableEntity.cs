using Genesis.Core;
using Gnosis.ECS.Entity;

namespace Genesis.Rendering;

public interface IRenderableEntity
{
    EntityId Id { get; }
    string Type { get; }
    Position Position { get; }
    CausalFeatures Features { get; }
    IReadOnlyDictionary<string, object> Properties { get; }
}

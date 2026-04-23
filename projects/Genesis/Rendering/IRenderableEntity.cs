using Genesis.Core;

namespace Genesis.Rendering;

public interface IRenderableEntity
{
    EntityId Id { get; }
    string Type { get; }
    Position Position { get; }
    CausalFeatures Features { get; }
    IReadOnlyDictionary<string, object> Properties { get; }
}

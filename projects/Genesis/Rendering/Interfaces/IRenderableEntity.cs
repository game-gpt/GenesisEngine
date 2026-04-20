using Genesis.Core.ValueObjects;

namespace Genesis.Rendering.Interfaces;

public interface IRenderableEntity
{
    EntityId Id { get; }
    string Type { get; }
    Position Position { get; }
    CausalFeatures Features { get; }
    IReadOnlyDictionary<string, object> Properties { get; }
}

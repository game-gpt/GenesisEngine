using Genesis.Core.ValueObjects;

namespace Genesis.Core.Interfaces;

public interface IEntity
{
    EntityId Id { get; }
}

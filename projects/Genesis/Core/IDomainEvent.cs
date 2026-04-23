using Gnosis.ECS.Entity;

namespace Genesis.Core;

public interface IDomainEvent
{
    EntityId AggregateId { get; }
    Timestamp OccurredOn { get; }
}

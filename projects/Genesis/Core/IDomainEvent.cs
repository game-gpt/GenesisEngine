using Gnosis.Core.Entity;

namespace Genesis.Core;

public interface IDomainEvent
{
    EntityId AggregateId { get; }
    Timestamp OccurredOn { get; }
}

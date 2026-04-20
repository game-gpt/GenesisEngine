using Genesis.Core.ValueObjects;

namespace Genesis.Core.Events;

public interface IDomainEvent
{
    EntityId AggregateId { get; }
    Timestamp OccurredOn { get; }
}

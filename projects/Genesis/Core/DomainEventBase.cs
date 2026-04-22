using Genesis.Core.ValueObjects;

namespace Genesis.Core.Events;

public abstract record DomainEventBase(EntityId AggregateId) : IDomainEvent
{
    public Timestamp OccurredOn { get; } = Timestamp.Now;
}

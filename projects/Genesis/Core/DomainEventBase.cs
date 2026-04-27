namespace Genesis.Core;

/// <summary>
/// 领域事件基类
/// </summary>
/// <param name="AggregateId">聚合根标识</param>
public abstract record DomainEventBase(EntityId AggregateId) : IDomainEvent
{
    /// <summary>
    /// 事件发生时间
    /// </summary>
    public Timestamp OccurredOn { get; } = Timestamp.Now;
}

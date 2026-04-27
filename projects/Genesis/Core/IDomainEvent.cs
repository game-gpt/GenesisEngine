namespace Genesis.Core;

/// <summary>
/// 领域事件接口
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// 聚合根标识
    /// </summary>
    EntityId AggregateId { get; }

    /// <summary>
    /// 事件发生时间
    /// </summary>
    Timestamp OccurredOn { get; }
}

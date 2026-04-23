using GnosisEcsEntityId = Gnosis.ECS.Entity.EntityId;

namespace Genesis.Core.Adapters;

/// <summary>
///     EntityId 适配器，在 Genesis 的 Guid 标识与 Gnosis.ECS 的 Index+Generation 标识之间转换
/// </summary>
public static class EntityIdAdapter
{
    /// <summary>
    ///     将 Genesis 的 EntityId 转换为 Gnosis.ECS 的 EntityId
    ///     使用 Guid 的哈希码作为 Index，Guid 的版本号作为 Generation
    /// </summary>
    public static GnosisEcsEntityId ToGnosisEntityId(this EntityId genesisId)
    {
        var guid = genesisId.Value;
        var index = (uint)(guid.GetHashCode() & 0x7FFFFFFF);
        var generation = (uint)((guid.Version << 28) | (guid.GetHashCode() >> 1 & 0x0FFFFFFF));
        return new GnosisEcsEntityId(index, generation);
    }

    /// <summary>
    ///     从 Gnosis.ECS 的 EntityId 恢复 Genesis 的 EntityId
    ///     需要通过映射表查找，因为 Guid 无法从 Index+Generation 还原
    /// </summary>
    public static EntityId ToGenesisEntityId(
        this GnosisEcsEntityId gnosisId,
        IReadOnlyDictionary<GnosisEcsEntityId, EntityId> mapping)
    {
        return mapping.TryGetValue(gnosisId, out var genesisId)
            ? genesisId
            : EntityId.Empty;
    }
}

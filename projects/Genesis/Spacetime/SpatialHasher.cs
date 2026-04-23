using Genesis.Core;

namespace Genesis.Spacetime;

/// <summary>
/// 空间哈希计算器，用于计算空间节点的哈希值
/// </summary>
public static class SpatialHasher
{
    /// <summary>
    /// 根据位置、节点层级和世界种子计算空间哈希值
    /// </summary>
    /// <param name="position">空间位置</param>
    /// <param name="level">节点层级</param>
    /// <param name="worldSeed">世界种子</param>
    /// <returns>计算得到的哈希值</returns>
    public static ulong ComputeSpatialHash(Position position, NodeLevel level, ulong worldSeed)
    {
        var xBits = (ulong)BitConverter.DoubleToInt64Bits(position.X);
        var yBits = (ulong)BitConverter.DoubleToInt64Bits(position.Y);
        var zBits = (ulong)BitConverter.DoubleToInt64Bits(position.Z);
        var levelBits = (ulong)(int)level;

        var hash = CombineHash(xBits, yBits);
        hash = CombineHash(hash, zBits);
        hash = CombineHash(hash, levelBits);
        hash = CombineHash(hash, worldSeed);

        return hash;
    }

    /// <summary>
    /// 组合两个哈希值，使用黄金比例散列常数进行混合
    /// </summary>
    /// <param name="h1">第一个哈希值</param>
    /// <param name="h2">第二个哈希值</param>
    /// <returns>组合后的哈希值</returns>
    public static ulong CombineHash(ulong h1, ulong h2)
    {
        return h1 ^ (h2 + 0x9e3779b97f4a7c15 + (h1 << 6) + (h1 >> 2));
    }

    /// <summary>
    /// 根据子节点的历史哈希值计算组合哈希值
    /// </summary>
    /// <param name="children">子节点列表</param>
    /// <returns>组合后的哈希值，若列表为空则返回 0</returns>
    public static ulong ComputeFromChildren(IReadOnlyList<ISpacetimeNode> children)
    {
        if (children.Count == 0)
        {
            return 0;
        }

        var hash = children[0].HistoryHash;

        for (var i = 1; i < children.Count; i++)
        {
            hash = CombineHash(hash, children[i].HistoryHash);
        }

        return hash;
    }
}

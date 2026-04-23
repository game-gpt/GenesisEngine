namespace Genesis.Spacetime;

/// <summary>
/// 基于 Merkle 风格的历史哈希链实现
/// </summary>
public class HistoryHashChain : IHistoryHash
{
    private ulong _value;

    /// <summary>
    /// 获取当前哈希值
    /// </summary>
    public ulong Value => _value;

    /// <summary>
    /// 使用新的哈希值更新当前值
    /// </summary>
    /// <param name="newHash">新的哈希值</param>
    public void Update(ulong newHash)
    {
        _value = newHash;
    }

    /// <summary>
    /// 将另一个哈希值与当前值组合，使用增强的异或位移公式
    /// </summary>
    /// <param name="otherHash">要组合的另一个哈希值</param>
    /// <returns>组合后的新哈希值</returns>
    public ulong Combine(ulong otherHash)
    {
        _value ^= otherHash + 0x9e3779b97f4a7c15 + (_value << 6) + (_value >> 2);
        return _value;
    }
}

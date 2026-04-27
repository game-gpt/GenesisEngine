namespace Genesis.Persistence;

/// <summary>
/// 历史存储接口
/// 内核通过此接口访问历史记录
/// </summary>
public interface IHistoryStore : IAsyncDisposable
{
    /// <summary>
    /// 追加历史记录
    /// </summary>
    /// <param name="historyHash">历史哈希</param>
    /// <param name="data">数据</param>
    Task AppendAsync(ulong historyHash, byte[] data);

    /// <summary>
    /// 获取历史记录
    /// </summary>
    /// <param name="historyHash">历史哈希</param>
    /// <returns>数据，不存在返回 null</returns>
    Task<byte[]?> GetAsync(ulong historyHash);

    /// <summary>
    /// 检查历史记录是否存在
    /// </summary>
    /// <param name="historyHash">历史哈希</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(ulong historyHash);

    /// <summary>
    /// 获取历史链
    /// </summary>
    /// <param name="startHash">起始哈希</param>
    /// <param name="maxDepth">最大深度</param>
    /// <returns>历史哈希链</returns>
    Task<IEnumerable<ulong>> GetHistoryChainAsync(ulong startHash, int maxDepth);
}

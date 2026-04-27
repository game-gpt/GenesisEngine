namespace Genesis.Persistence;

/// <summary>
/// 基于键值数据库的持久化历史存储
/// 使用 Genesis.Persistence.IKvDatabase 抽象，不直接依赖 Gnosis.Database
/// </summary>
public sealed class SolidHistoryStore : IHistoryStore, IAsyncDisposable
{
    #region 字段

    private readonly IKvDatabase _db;
    private bool _disposed;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化历史存储
    /// </summary>
    /// <param name="database">键值数据库</param>
    public SolidHistoryStore(IKvDatabase database)
    {
        _db = database;
    }

    #endregion

    #region IHistoryStore 实现

    /// <summary>
    /// 追加历史记录
    /// </summary>
    public async Task AppendAsync(ulong historyHash, byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = BitConverter.GetBytes(historyHash);
        await _db.PutAsync(key, data);
    }

    /// <summary>
    /// 获取历史记录
    /// </summary>
    public async Task<byte[]?> GetAsync(ulong historyHash)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = BitConverter.GetBytes(historyHash);
        return await _db.GetAsync(key);
    }

    /// <summary>
    /// 检查历史记录是否存在
    /// </summary>
    public async Task<bool> ExistsAsync(ulong historyHash)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = BitConverter.GetBytes(historyHash);
        return await _db.ExistsAsync(key);
    }

    /// <summary>
    /// 获取历史链
    /// </summary>
    public async Task<IEnumerable<ulong>> GetHistoryChainAsync(ulong startHash, int maxDepth)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var chain = new List<ulong>();
        var current = startHash;
        var depth = 0;

        while (depth < maxDepth)
        {
            var exists = await ExistsAsync(current);
            if (!exists)
            {
                break;
            }

            chain.Add(current);
            depth++;

            var data = await GetAsync(current);
            if (data is null || data.Length < 8)
            {
                break;
            }

            current = BitConverter.ToUInt64(data, 0);
        }

        return chain;
    }

    #endregion

    #region IAsyncDisposable

    /// <summary>
    /// 异步释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await _db.DisposeAsync();
    }

    #endregion
}

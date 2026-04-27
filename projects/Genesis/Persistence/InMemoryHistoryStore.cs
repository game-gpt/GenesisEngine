namespace Genesis.Persistence;

/// <summary>
/// 内存历史存储实现
/// 用于测试和不需要持久化的场景
/// </summary>
public sealed class InMemoryHistoryStore : IHistoryStore
{
    #region 字段

    private readonly Dictionary<ulong, byte[]> _data = new();
    private bool _disposed;

    #endregion

    #region IHistoryStore 实现

    /// <summary>
    /// 追加历史记录
    /// </summary>
    public Task AppendAsync(ulong historyHash, byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _data[historyHash] = data;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取历史记录
    /// </summary>
    public Task<byte[]?> GetAsync(ulong historyHash)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Task.FromResult(_data.GetValueOrDefault(historyHash));
    }

    /// <summary>
    /// 检查历史记录是否存在
    /// </summary>
    public Task<bool> ExistsAsync(ulong historyHash)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Task.FromResult(_data.ContainsKey(historyHash));
    }

    /// <summary>
    /// 获取历史链
    /// </summary>
    public Task<IEnumerable<ulong>> GetHistoryChainAsync(ulong startHash, int maxDepth)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var chain = new List<ulong>();
        var current = startHash;
        var depth = 0;

        while (depth < maxDepth && _data.ContainsKey(current))
        {
            chain.Add(current);
            depth++;

            var data = _data[current];
            if (data is null || data.Length < 8)
            {
                break;
            }

            current = BitConverter.ToUInt64(data, 0);
        }

        return Task.FromResult<IEnumerable<ulong>>(chain);
    }

    #endregion

    #region IAsyncDisposable

    /// <summary>
    /// 释放资源
    /// </summary>
    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;
        _disposed = true;
        _data.Clear();
        return ValueTask.CompletedTask;
    }

    #endregion
}

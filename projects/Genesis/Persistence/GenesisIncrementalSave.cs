namespace Genesis.Persistence;

/// <summary>
/// 增量存档
/// 使用 Genesis.Persistence.IKvDatabase 抽象，不直接依赖 Gnosis.Database
/// </summary>
public sealed class GenesisIncrementalSave : IIncrementalSave, IAsyncDisposable
{
    #region 字段

    private readonly IKvDatabase _db;
    private readonly IHistoryStore _historyStore;
    private readonly Dictionary<string, byte[]> _pendingSaves = new();
    private bool _disposed;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化增量存档
    /// </summary>
    /// <param name="database">键值数据库</param>
    /// <param name="historyStore">历史存储</param>
    public GenesisIncrementalSave(IKvDatabase database, IHistoryStore historyStore)
    {
        _db = database;
        _historyStore = historyStore;
    }

    #endregion

    #region IIncrementalSave 实现

    /// <summary>
    /// 保存所有暂存数据
    /// </summary>
    public async Task SaveAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_pendingSaves.Count == 0)
        {
            return;
        }

        foreach (var (key, data) in _pendingSaves)
        {
            var dbKey = System.Text.Encoding.UTF8.GetBytes(key);
            await _db.PutAsync(dbKey, data);
        }

        _pendingSaves.Clear();
    }

    /// <summary>
    /// 保存并创建检查点
    /// </summary>
    public async Task SaveAsync(string checkpointName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await SaveAsync();

        var checkpointData = SerializeCheckpointMetadata(checkpointName);
        var checkpointHash = ComputeHash(checkpointName);
        await _historyStore.AppendAsync(checkpointHash, checkpointData);
    }

    /// <summary>
    /// 加载检查点
    /// </summary>
    public async Task<bool> LoadAsync(string checkpointName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var checkpointHash = ComputeHash(checkpointName);
        var exists = await _historyStore.ExistsAsync(checkpointHash);
        return exists;
    }

    /// <summary>
    /// 获取所有检查点
    /// </summary>
    public Task<IEnumerable<string>> GetCheckpointsAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var result = _pendingSaves.Keys
            .Where(k => k.StartsWith("checkpoint:"))
            .Select(k => k["checkpoint:".Length..])
            .ToList();

        return Task.FromResult<IEnumerable<string>>(result);
    }

    /// <summary>
    /// 删除检查点
    /// </summary>
    public async Task DeleteCheckpointAsync(string checkpointName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = System.Text.Encoding.UTF8.GetBytes($"checkpoint:{checkpointName}");
        await _db.DeleteAsync(key);
    }

    #endregion

    #region 公开方法

    /// <summary>
    /// 暂存数据
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    public void Stage(string key, byte[] data)
    {
        _pendingSaves[key] = data;
    }

    /// <summary>
    /// 批量暂存数据
    /// </summary>
    /// <param name="entries">键值对列表</param>
    public void StageRange(IEnumerable<(string Key, byte[] Data)> entries)
    {
        foreach (var (key, data) in entries)
        {
            _pendingSaves[key] = data;
        }
    }

    #endregion

    #region 私有方法

    private static ulong ComputeHash(string name)
    {
        ulong hash = 14695981039346656037;
        foreach (var c in name)
        {
            hash ^= (ulong)c;
            hash *= 1099511628211;
        }

        return hash;
    }

    private static byte[] SerializeCheckpointMetadata(string name)
    {
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
        var result = new byte[4 + nameBytes.Length];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(result, nameBytes.Length);
        Array.Copy(nameBytes, 0, result, 4, nameBytes.Length);
        return result;
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

        if (_pendingSaves.Count > 0)
        {
            await SaveAsync();
        }

        await _db.DisposeAsync();
    }

    #endregion
}

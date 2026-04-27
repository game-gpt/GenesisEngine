namespace Genesis.Persistence;

/// <summary>
/// 增量存档实现
/// 使用本地 IGenesisKvStore 接口，不直接依赖 Gnosis.Database.Core
/// </summary>
public sealed class GenesisIncrementalSave : IIncrementalSave
{
    #region 字段

    private readonly IGenesisKvStore _db;
    private readonly IHistoryStore _historyStore;
    private readonly bool _ownsDatabase;
    private readonly Dictionary<string, byte[]> _pendingSaves = new();
    private bool _disposed;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化增量存档（使用外部数据库和历史存储）
    /// </summary>
    /// <param name="database">键值数据库</param>
    /// <param name="historyStore">历史存储</param>
    /// <param name="ownsDatabase">是否拥有数据库生命周期</param>
    public GenesisIncrementalSave(IGenesisKvStore database, IHistoryStore historyStore, bool ownsDatabase = false)
    {
        _db = database;
        _historyStore = historyStore;
        _ownsDatabase = ownsDatabase;
    }

    /// <summary>
    /// 初始化增量存档（使用内存历史存储）
    /// </summary>
    /// <param name="database">键值数据库</param>
    /// <param name="ownsDatabase">是否拥有数据库生命周期</param>
    public GenesisIncrementalSave(IGenesisKvStore database, bool ownsDatabase = false)
    {
        _db = database;
        _historyStore = new InMemoryHistoryStore();
        _ownsDatabase = ownsDatabase;
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
            await _db.PutAsync(key, data);
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
    public async Task<IEnumerable<string>> GetCheckpointsAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var prefix = "checkpoint:";
        var result = new List<string>();

        var checkpointKey = await _db.GetAsync($"{prefix}index");
        if (checkpointKey is not null)
        {
            var names = System.Text.Encoding.UTF8.GetString(checkpointKey);
            result.AddRange(names.Split('\0', StringSplitOptions.RemoveEmptyEntries));
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// 删除检查点
    /// </summary>
    public async Task DeleteCheckpointAsync(string checkpointName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = $"checkpoint:{checkpointName}";
        await _db.DeleteAsync(key);
    }

    /// <summary>
    /// 暂存数据
    /// </summary>
    public void Stage(string key, byte[] data)
    {
        _pendingSaves[key] = data;
    }

    /// <summary>
    /// 批量暂存数据
    /// </summary>
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
    /// 释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_pendingSaves.Count > 0)
        {
            await SaveAsync();
        }

        if (_ownsDatabase)
        {
            _db.Dispose();
        }
    }

    #endregion
}

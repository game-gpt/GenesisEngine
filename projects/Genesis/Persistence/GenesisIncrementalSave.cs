using Gnosis.Database.Core;

namespace Genesis.Persistence;

public sealed class GenesisIncrementalSave : IIncrementalSave, IAsyncDisposable
{
    #region 字段

    private readonly IKvDatabase _db;
    private readonly IHistoryStore _historyStore;
    private readonly Dictionary<string, byte[]> _pendingSaves = new();
    private bool _disposed;

    #endregion

    #region 构造函数

    public GenesisIncrementalSave(IKvDatabase database, IHistoryStore historyStore)
    {
        _db = database;
        _historyStore = historyStore;
    }

    public GenesisIncrementalSave(string path = ".genesis/saves")
    {
        var options = new DatabaseOptions { Path = path };
        _db = new GenesisKvDatabase(options);
        _historyStore = new InMemoryHistoryStore();
    }

    #endregion

    #region IIncrementalSave 实现

    public async Task SaveAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_pendingSaves.Count == 0)
        {
            return;
        }

        foreach (var (key, data) in _pendingSaves)
        {
            var dbKey = DatabaseKey.FromString(key);
            var dbValue = new DatabaseValue(data);
            await _db.PutAsync(dbKey, dbValue);
        }

        _pendingSaves.Clear();
    }

    public async Task SaveAsync(string checkpointName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await SaveAsync();

        var checkpointData = SerializeCheckpointMetadata(checkpointName);
        var checkpointHash = ComputeHash(checkpointName);
        await _historyStore.AppendAsync(checkpointHash, checkpointData);
    }

    public async Task<bool> LoadAsync(string checkpointName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var checkpointHash = ComputeHash(checkpointName);
        var exists = await _historyStore.ExistsAsync(checkpointHash);
        if (!exists)
        {
            return false;
        }

        return true;
    }

    public async Task<IEnumerable<string>> GetCheckpointsAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var result = new List<string>();
        using var cursor = _db.Seek(DatabaseKey.Empty);

        while (cursor.IsValid)
        {
            var keyStr = cursor.Current.Key.ToString();
            if (keyStr.StartsWith("checkpoint:"))
            {
                result.Add(keyStr["checkpoint:".Length..]);
            }
            cursor.MoveNext();
        }

        return await Task.FromResult(result);
    }

    public async Task DeleteCheckpointAsync(string checkpointName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = DatabaseKey.FromString($"checkpoint:{checkpointName}");
        await _db.DeleteAsync(key);
    }

    #endregion

    #region 公开方法

    public void Stage(string key, byte[] data)
    {
        _pendingSaves[key] = data;
    }

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

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_pendingSaves.Count > 0)
        {
            await SaveAsync();
        }

        _db.Dispose();
    }

    #endregion
}

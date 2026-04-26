using Gnosis.Database.Core;

namespace Genesis.Persistence;

public sealed class SolidHistoryStore : IHistoryStore, IAsyncDisposable
{
    #region 字段

    private readonly IKvDatabase _db;
    private bool _disposed;

    #endregion

    #region 构造函数

    public SolidHistoryStore(string path = ".genesis/history")
    {
        var options = new DatabaseOptions
        {
            Path = path
        };
        _db = new GenesisKvDatabase(options);
    }

    public SolidHistoryStore(IKvDatabase database)
    {
        _db = database;
    }

    #endregion

    #region IHistoryStore 实现

    public async Task AppendAsync(ulong historyHash, byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = DatabaseKey.FromUInt64(historyHash);
        var value = new DatabaseValue(data);
        await _db.PutAsync(key, value);
    }

    public async Task<byte[]?> GetAsync(ulong historyHash)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = DatabaseKey.FromUInt64(historyHash);
        var result = await _db.GetAsync(key);
        if (!result.HasValue)
        {
            return null;
        }

        return result.Value.Bytes.ToArray();
    }

    public async Task<bool> ExistsAsync(ulong historyHash)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = DatabaseKey.FromUInt64(historyHash);
        return await _db.ExistsAsync(key);
    }

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

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        _db.Dispose();
        await ValueTask.CompletedTask;
    }

    #endregion
}

using Genesis.Core.ValueObjects;
using Genesis.Persistence.Interfaces;
using SolidDB.Core;

namespace Genesis.Persistence.Implementations;

public sealed class SolidHistoryStore : IHistoryStore, IAsyncDisposable
{
    #region 字段

    private readonly SolidDB.SolidDatabase _db;
    private bool _disposed;

    #endregion

    #region 构造函数

    public SolidHistoryStore(string path = ".genesis/history")
    {
        _db = new SolidDB.SolidDatabase(new SolidOptions
        {
            Path = path
        });
    }

    #endregion

    #region IHistoryStore 实现

    public async Task AppendAsync(ulong historyHash, byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = SolidKey.FromUInt64(historyHash);
        var value = new SolidValue(data);
        await _db.PutAsync(key, value);
    }

    public async Task<byte[]?> GetAsync(ulong historyHash)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = SolidKey.FromUInt64(historyHash);
        var result = await _db.GetAsync<SolidValue>(key);
        if (result.IsEmpty)
        {
            return null;
        }

        return result.Bytes.ToArray();
    }

    public async Task<bool> ExistsAsync(ulong historyHash)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = SolidKey.FromUInt64(historyHash);
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

        await _db.DisposeAsync();
    }

    #endregion
}

using Gnosis.Database.Core;

namespace Genesis.Persistence;

public sealed class GenesisKvDatabase : IKvDatabase
{
    #region 字段

    private readonly Gnosis.Database.Engine.GenesisKvDatabase _inner;
    private bool _disposed;

    #endregion

    #region 构造函数

    public GenesisKvDatabase(DatabaseOptions options)
    {
        _inner = new Gnosis.Database.Engine.GenesisKvDatabase(options);
        Options = options;
    }

    #endregion

    #region 属性

    public DatabaseOptions Options { get; }

    public DatabaseStatistics Statistics => _inner.Statistics;

    #endregion

    #region IKvDatabase 实现

    public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
    {
        return _inner.BeginTransaction(isolationLevel);
    }

    public ISnapshot CreateSnapshot()
    {
        return _inner.CreateSnapshot();
    }

    public ValueTask<DatabaseValue?> GetAsync(DatabaseKey key, CancellationToken cancellationToken = default)
    {
        return _inner.GetAsync(key, cancellationToken);
    }

    public ValueTask PutAsync(DatabaseKey key, DatabaseValue value, CancellationToken cancellationToken = default)
    {
        return _inner.PutAsync(key, value, cancellationToken);
    }

    public ValueTask<bool> DeleteAsync(DatabaseKey key, CancellationToken cancellationToken = default)
    {
        return _inner.DeleteAsync(key, cancellationToken);
    }

    public ValueTask<bool> ExistsAsync(DatabaseKey key, CancellationToken cancellationToken = default)
    {
        return _inner.ExistsAsync(key, cancellationToken);
    }

    public ICursor Seek(DatabaseKey key)
    {
        return _inner.Seek(key);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _inner.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;
        _disposed = true;
        return _inner.DisposeAsync();
    }

    #endregion
}

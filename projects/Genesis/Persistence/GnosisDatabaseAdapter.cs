using Gnosis.Database.Core;

namespace Genesis.Persistence;

/// <summary>
/// Gnosis 数据库适配器
/// 将 Gnosis.Database.Engine.GenesisKvDatabase 适配为 Genesis.Persistence.IKvDatabase
/// 此文件是唯一允许直接引用 Gnosis.Database 类型的 Persistence 文件
/// </summary>
public sealed class GnosisDatabaseAdapter : IKvDatabase
{
    #region 字段

    private readonly Gnosis.Database.Engine.GenesisKvDatabase _inner;
    private bool _disposed;

    #endregion

    #region 属性

    /// <summary>
    /// 是否已打开
    /// </summary>
    public bool IsOpen => !_disposed;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 Gnosis 数据库适配器
    /// </summary>
    /// <param name="path">数据库路径</param>
    /// <param name="createIfNotExists">不存在时是否创建</param>
    public GnosisDatabaseAdapter(string path = ".genesis/db", bool createIfNotExists = true)
    {
        var options = new DatabaseOptions { Path = path };
        _inner = new Gnosis.Database.Engine.GenesisKvDatabase(options);
    }

    /// <summary>
    /// 从 Gnosis 数据库实例创建适配器
    /// </summary>
    /// <param name="inner">Gnosis 数据库实例</param>
    public GnosisDatabaseAdapter(Gnosis.Database.Engine.GenesisKvDatabase inner)
    {
        _inner = inner;
    }

    #endregion

    #region IKvDatabase 实现

    /// <summary>
    /// 打开数据库
    /// </summary>
    public Task OpenAsync(string path, bool createIfNotExists = true)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 关闭数据库
    /// </summary>
    public Task CloseAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    public async Task<byte[]?> GetAsync(byte[] key, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var dbKey = DatabaseKey.FromBytes(key);
        var result = await _inner.GetAsync(dbKey, cancellationToken);
        if (!result.HasValue)
        {
            return null;
        }

        return result.Value.Bytes.ToArray();
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    public async Task PutAsync(byte[] key, byte[] value, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var dbKey = DatabaseKey.FromBytes(key);
        var dbValue = new DatabaseValue(value);
        await _inner.PutAsync(dbKey, dbValue, cancellationToken);
    }

    /// <summary>
    /// 删除数据
    /// </summary>
    public async Task<bool> DeleteAsync(byte[] key, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var dbKey = DatabaseKey.FromBytes(key);
        return await _inner.DeleteAsync(dbKey, cancellationToken);
    }

    /// <summary>
    /// 检查键是否存在
    /// </summary>
    public async Task<bool> ExistsAsync(byte[] key, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var dbKey = DatabaseKey.FromBytes(key);
        return await _inner.ExistsAsync(dbKey, cancellationToken);
    }

    /// <summary>
    /// 刷新到磁盘
    /// </summary>
    public Task FlushAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Task.CompletedTask;
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _inner.Dispose();
    }

    /// <summary>
    /// 异步释放资源
    /// </summary>
    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;
        _disposed = true;
        return _inner.DisposeAsync();
    }

    #endregion
}

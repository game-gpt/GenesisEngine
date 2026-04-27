using Gnosis.Database.Core;

namespace Genesis.Persistence;

/// <summary>
/// IKvDatabase 的 Gnosis.Database 适配器
/// 将 Gnosis.Database.Core.IKvDatabase 的调用桥接到本地 IKvDatabase 接口
/// </summary>
public sealed class GnosisKvDatabaseAdapter : IKvDatabase
{
    #region 字段

    private readonly Gnosis.Database.Core.IKvDatabase _inner;
    private bool _disposed;

    #endregion

    #region 属性

    /// <summary>
    /// 底层 Gnosis 数据库实例
    /// </summary>
    public Gnosis.Database.Core.IKvDatabase Inner => _inner;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 Gnosis 键值数据库适配器
    /// </summary>
    /// <param name="inner">Gnosis IKvDatabase 实例</param>
    public GnosisKvDatabaseAdapter(Gnosis.Database.Core.IKvDatabase inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
    }

    /// <summary>
    /// 使用默认选项创建 Gnosis 键值数据库适配器
    /// </summary>
    /// <param name="path">数据库路径</param>
    public GnosisKvDatabaseAdapter(string path)
    {
        var options = new DatabaseOptions { Path = path };
        _inner = new Gnosis.Database.Engine.GenesisKvDatabase(options);
    }

    #endregion

    #region IKvDatabase 实现

    /// <summary>
    /// 获取值
    /// </summary>
    public async ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var dbKey = DatabaseKey.FromString(key);
        var result = await _inner.GetAsync(dbKey, cancellationToken);

        if (!result.HasValue)
        {
            return null;
        }

        return result.Value.Bytes.ToArray();
    }

    /// <summary>
    /// 设置值
    /// </summary>
    public async ValueTask PutAsync(string key, byte[] value, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var dbKey = DatabaseKey.FromString(key);
        var dbValue = new DatabaseValue(value);
        await _inner.PutAsync(dbKey, dbValue, cancellationToken);
    }

    /// <summary>
    /// 删除键
    /// </summary>
    public async ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var dbKey = DatabaseKey.FromString(key);
        return await _inner.DeleteAsync(dbKey, cancellationToken);
    }

    /// <summary>
    /// 检查键是否存在
    /// </summary>
    public async ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var dbKey = DatabaseKey.FromString(key);
        return await _inner.ExistsAsync(dbKey, cancellationToken);
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

    #endregion

    #region IAsyncDisposable

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

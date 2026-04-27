namespace Genesis.Persistence;

/// <summary>
/// 键值数据库抽象接口
/// 内核通过此接口访问持久化存储，不直接依赖 Gnosis.Database
/// Gnosis 实现通过 GnosisDatabaseAdapter 适配
/// </summary>
public interface IKvDatabase : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// 是否已打开
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// 打开数据库
    /// </summary>
    /// <param name="path">数据库路径</param>
    /// <param name="createIfNotExists">不存在时是否创建</param>
    Task OpenAsync(string path, bool createIfNotExists = true);

    /// <summary>
    /// 关闭数据库
    /// </summary>
    Task CloseAsync();

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>值，不存在返回 null</returns>
    Task<byte[]?> GetAsync(byte[] key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PutAsync(byte[] key, byte[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(byte[] key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查键是否存在
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(byte[] key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新到磁盘
    /// </summary>
    Task FlushAsync();
}

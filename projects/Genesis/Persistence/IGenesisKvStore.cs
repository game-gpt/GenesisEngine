namespace Genesis.Persistence;

/// <summary>
/// Genesis 键值存储接口
/// 内核通过此接口访问持久化存储，不直接依赖 Gnosis.Database
/// Gnosis 实现通过 GnosisKvDatabaseAdapter 适配
/// </summary>
public interface IGenesisKvStore : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>值，不存在返回 null</returns>
    ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令牌</param>
    ValueTask PutAsync(string key, byte[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除键
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否删除成功</returns>
    ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查键是否存在
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在</returns>
    ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

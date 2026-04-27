namespace Genesis.Persistence;

/// <summary>
/// 增量存档接口
/// 内核通过此接口执行存档操作
/// </summary>
public interface IIncrementalSave : IAsyncDisposable
{
    /// <summary>
    /// 保存所有暂存数据
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// 保存并创建检查点
    /// </summary>
    /// <param name="checkpointName">检查点名称</param>
    Task SaveAsync(string checkpointName);

    /// <summary>
    /// 加载检查点
    /// </summary>
    /// <param name="checkpointName">检查点名称</param>
    /// <returns>是否加载成功</returns>
    Task<bool> LoadAsync(string checkpointName);

    /// <summary>
    /// 获取所有检查点
    /// </summary>
    /// <returns>检查点名称列表</returns>
    Task<IEnumerable<string>> GetCheckpointsAsync();

    /// <summary>
    /// 删除检查点
    /// </summary>
    /// <param name="checkpointName">检查点名称</param>
    Task DeleteCheckpointAsync(string checkpointName);

    /// <summary>
    /// 暂存数据
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    void Stage(string key, byte[] data);

    /// <summary>
    /// 批量暂存数据
    /// </summary>
    /// <param name="entries">键值对列表</param>
    void StageRange(IEnumerable<(string Key, byte[] Data)> entries);
}

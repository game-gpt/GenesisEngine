namespace Genesis.HAL;

/// <summary>
/// 音频设备抽象接口
/// 内核通过此接口执行音频操作，不直接依赖 Gnosis.Audio
/// HAL 实现负责将调用委托给 Gnosis.Audio
/// </summary>
public interface IAudioDevice
{
    /// <summary>
    /// 是否已初始化
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// 全局音量
    /// </summary>
    float GlobalVolume { get; set; }

    /// <summary>
    /// 初始化音频设备
    /// </summary>
    void Initialize();

    /// <summary>
    /// 关闭音频设备
    /// </summary>
    void Shutdown();

    /// <summary>
    /// 帧更新
    /// </summary>
    /// <param name="delta">帧间隔时间（秒）</param>
    void Update(float delta);

    /// <summary>
    /// 创建音频总线
    /// </summary>
    /// <param name="name">总线名称</param>
    /// <param name="parentName">父总线名称，为 null 时挂载到主总线</param>
    /// <returns>音频总线</returns>
    IAudioBus CreateBus(string name, string? parentName = null);

    /// <summary>
    /// 获取音频总线
    /// </summary>
    /// <param name="name">总线名称</param>
    /// <returns>音频总线，未找到返回 null</returns>
    IAudioBus? GetBus(string name);

    /// <summary>
    /// 销毁音频总线
    /// </summary>
    /// <param name="name">总线名称</param>
    void DestroyBus(string name);
}

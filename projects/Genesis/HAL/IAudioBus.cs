namespace Genesis.HAL;

/// <summary>
/// 音频总线抽象接口
/// </summary>
public interface IAudioBus
{
    /// <summary>
    /// 总线名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 音量
    /// </summary>
    float Volume { get; set; }
}

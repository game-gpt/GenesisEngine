namespace Genesis.HAL;

/// <summary>
/// 硬件信息抽象接口
/// 编译期注入的静态信息，游戏代码直接使用
/// 对应 GGScript 的 HAL.hardware
/// </summary>
public interface IHardwareInfo
{
    /// <summary>
    /// GPU 名称
    /// </summary>
    string GpuName { get; }

    /// <summary>
    /// GPU 显存大小（MB）
    /// </summary>
    int VramMb { get; }

    /// <summary>
    /// 图形后端类型
    /// </summary>
    string GraphicsBackend { get; }

    /// <summary>
    /// CPU 核心数
    /// </summary>
    int CpuCores { get; }

    /// <summary>
    /// 系统内存大小（MB）
    /// </summary>
    int SystemMemoryMb { get; }

    /// <summary>
    /// 最大纹理尺寸
    /// </summary>
    int MaxTextureSize { get; }

    /// <summary>
    /// 显示器宽度（像素）
    /// </summary>
    int DisplayWidth { get; }

    /// <summary>
    /// 显示器高度（像素）
    /// </summary>
    int DisplayHeight { get; }

    /// <summary>
    /// 显示器刷新率（Hz）
    /// </summary>
    int RefreshRate { get; }
}

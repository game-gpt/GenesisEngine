using Gnosis.PAL;

namespace Genesis.HAL.Adapters;

/// <summary>
/// IHardwareInfo 的 Gnosis.PAL.HardwareInfo 适配器
/// </summary>
public sealed class GnosisHardwareInfoAdapter : IHardwareInfo
{
    #region 字段

    private readonly HardwareInfo _info;

    #endregion

    #region 属性

    /// <summary>
    /// GPU 名称
    /// </summary>
    public string GpuName => _info.GpuName;

    /// <summary>
    /// GPU 显存大小（MB）
    /// </summary>
    public int VramMb => _info.VramMb;

    /// <summary>
    /// 图形后端类型
    /// </summary>
    public string GraphicsBackend => _info.GraphicsBackend.ToString();

    /// <summary>
    /// CPU 核心数
    /// </summary>
    public int CpuCores => _info.CpuCores;

    /// <summary>
    /// 系统内存大小（MB）
    /// </summary>
    public int SystemMemoryMb => _info.SystemMemoryMb;

    /// <summary>
    /// 最大纹理尺寸
    /// </summary>
    public int MaxTextureSize => _info.MaxTextureSize;

    /// <summary>
    /// 显示器宽度（像素）
    /// </summary>
    public int DisplayWidth => _info.DisplayWidth;

    /// <summary>
    /// 显示器高度（像素）
    /// </summary>
    public int DisplayHeight => _info.DisplayHeight;

    /// <summary>
    /// 显示器刷新率（Hz）
    /// </summary>
    public int RefreshRate => _info.RefreshRate;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化硬件信息适配器
    /// </summary>
    /// <param name="info">Gnosis 硬件信息</param>
    public GnosisHardwareInfoAdapter(HardwareInfo info)
    {
        _info = info;
    }

    /// <summary>
    /// 从当前系统检测创建适配器
    /// </summary>
    /// <returns>硬件信息适配器</returns>
    public static GnosisHardwareInfoAdapter Detect()
    {
        return new GnosisHardwareInfoAdapter(HardwareInfo.Detect());
    }

    #endregion
}

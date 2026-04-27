using Gnosis.PAL;

namespace Genesis.HAL;

/// <summary>
/// Genesis HAL 全局访问点
/// 编译期绑定的硬件能力，运行时不可变
/// 对应 GGScript 的 HAL.use::<T>() 和 HAL.hardware
/// </summary>
public static class GenesisHAL
{
    #region 字段

    private static IEntityWorld? _entityWorld;
    private static IRenderer? _renderer;
    private static IAudioDevice? _audioDevice;
    private static IInputManager? _inputManager;
    private static IFileSystem? _fileSystem;
    private static IHardwareInfo? _hardwareInfo;
    private static CapabilityBus? _capabilityBus;

    #endregion

    #region 属性

    /// <summary>
    /// 实体世界（编译期绑定，总是存在）
    /// </summary>
    public static IEntityWorld EntityWorld => _entityWorld ?? throw new InvalidOperationException("HAL 未初始化：EntityWorld 未注册");

    /// <summary>
    /// 渲染器（编译期绑定，总是存在）
    /// </summary>
    public static IRenderer Renderer => _renderer ?? throw new InvalidOperationException("HAL 未初始化：Renderer 未注册");

    /// <summary>
    /// 音频设备（编译期绑定，总是存在）
    /// </summary>
    public static IAudioDevice AudioDevice => _audioDevice ?? throw new InvalidOperationException("HAL 未初始化：AudioDevice 未注册");

    /// <summary>
    /// 输入管理器（编译期绑定，总是存在）
    /// </summary>
    public static IInputManager InputManager => _inputManager ?? throw new InvalidOperationException("HAL 未初始化：InputManager 未注册");

    /// <summary>
    /// 文件系统（编译期绑定，总是存在）
    /// </summary>
    public static IFileSystem FileSystem => _fileSystem ?? throw new InvalidOperationException("HAL 未初始化：FileSystem 未注册");

    /// <summary>
    /// 硬件信息（编译期注入，总是存在）
    /// 对应 GGScript 的 HAL.hardware
    /// </summary>
    public static IHardwareInfo Hardware => _hardwareInfo ?? throw new InvalidOperationException("HAL 未初始化：Hardware 未注册");

    /// <summary>
    /// PAL 能力总线（运行期查询）
    /// 对应 GGScript 的 PAL.use::<T>()
    /// </summary>
    public static CapabilityBus Capabilities => _capabilityBus ?? throw new InvalidOperationException("HAL 未初始化：Capabilities 未注册");

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public static bool IsInitialized { get; private set; }

    #endregion

    #region 注册

    /// <summary>
    /// 注册实体世界
    /// </summary>
    /// <param name="entityWorld">实体世界实现</param>
    public static void RegisterEntityWorld(IEntityWorld entityWorld)
    {
        ArgumentNullException.ThrowIfNull(entityWorld);
        _entityWorld = entityWorld;
    }

    /// <summary>
    /// 注册渲染器
    /// </summary>
    /// <param name="renderer">渲染器实现</param>
    public static void RegisterRenderer(IRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _renderer = renderer;
    }

    /// <summary>
    /// 注册音频设备
    /// </summary>
    /// <param name="audioDevice">音频设备实现</param>
    public static void RegisterAudioDevice(IAudioDevice audioDevice)
    {
        ArgumentNullException.ThrowIfNull(audioDevice);
        _audioDevice = audioDevice;
    }

    /// <summary>
    /// 注册输入管理器
    /// </summary>
    /// <param name="inputManager">输入管理器实现</param>
    public static void RegisterInputManager(IInputManager inputManager)
    {
        ArgumentNullException.ThrowIfNull(inputManager);
        _inputManager = inputManager;
    }

    /// <summary>
    /// 注册文件系统
    /// </summary>
    /// <param name="fileSystem">文件系统实现</param>
    public static void RegisterFileSystem(IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// 注册硬件信息
    /// </summary>
    /// <param name="hardwareInfo">硬件信息实现</param>
    public static void RegisterHardware(IHardwareInfo hardwareInfo)
    {
        ArgumentNullException.ThrowIfNull(hardwareInfo);
        _hardwareInfo = hardwareInfo;
    }

    /// <summary>
    /// 注册 PAL 能力总线
    /// </summary>
    /// <param name="capabilityBus">能力总线</param>
    public static void RegisterCapabilities(CapabilityBus capabilityBus)
    {
        ArgumentNullException.ThrowIfNull(capabilityBus);
        _capabilityBus = capabilityBus;
    }

    #endregion

    #region 查询

    /// <summary>
    /// 获取 HAL 能力（编译期保证，不存在则抛出异常）
    /// 对应 GGScript 的 HAL.use::<T>()
    /// </summary>
    /// <typeparam name="T">能力接口类型</typeparam>
    /// <returns>能力实例</returns>
    public static T Use<T>() where T : class
    {
        if (typeof(T) == typeof(IEntityWorld) && _entityWorld is T ew)
        {
            return ew;
        }

        if (typeof(T) == typeof(IRenderer) && _renderer is T r)
        {
            return r;
        }

        if (typeof(T) == typeof(IAudioDevice) && _audioDevice is T ad)
        {
            return ad;
        }

        if (typeof(T) == typeof(IInputManager) && _inputManager is T im)
        {
            return im;
        }

        if (typeof(T) == typeof(IFileSystem) && _fileSystem is T fs)
        {
            return fs;
        }

        if (typeof(T) == typeof(IHardwareInfo) && _hardwareInfo is T hi)
        {
            return hi;
        }

        throw new InvalidOperationException($"HAL 能力 {typeof(T).Name} 未注册");
    }

    /// <summary>
    /// 尝试获取 HAL 能力
    /// </summary>
    /// <typeparam name="T">能力接口类型</typeparam>
    /// <returns>能力实例，未注册返回 null</returns>
    public static T? TryUse<T>() where T : class
    {
        try
        {
            return Use<T>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    #endregion

    #region 初始化

    /// <summary>
    /// 标记 HAL 初始化完成
    /// </summary>
    public static void MarkInitialized()
    {
        IsInitialized = true;
    }

    /// <summary>
    /// 重置 HAL（仅用于测试）
    /// </summary>
    public static void Reset()
    {
        _entityWorld = null;
        _renderer = null;
        _audioDevice = null;
        _inputManager = null;
        _fileSystem = null;
        _hardwareInfo = null;
        _capabilityBus = null;
        IsInitialized = false;
    }

    #endregion
}

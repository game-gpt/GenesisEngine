using Genesis.HAL;
using Gnosis.PAL;
using Gnosis.Runtime.VM;

namespace Genesis.Runtime;

/// <summary>
/// HAL/PAL 原生函数桥接
/// 将 HAL.use/PAL.use/HAL.hardware/PAL.platform 注册为 VM 原生函数
/// 使 GGScript 脚本能够通过 native call 访问 HAL/PAL 能力
/// </summary>
public sealed class HalPalNativeBridge
{
    #region 常量

    private const int BaseId = 11000;

    #endregion

    #region 字段

    private readonly NativeFunctionRegistry _registry;

    #endregion

    #region 构造函数

    public HalPalNativeBridge(NativeFunctionRegistry registry)
    {
        _registry = registry;
    }

    #endregion

    #region 注册

    public void RegisterAll()
    {
        RegisterHalFunctions();
        RegisterHardwareFieldFunctions();
        RegisterPalFunctions();
        RegisterPlatformFieldFunctions();
    }

    #endregion

    #region HAL 原生函数

    private void RegisterHalFunctions()
    {
        _registry.Register(new HalPalNativeFunction(BaseId + 1, "hal_hardware", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.Null;
            }

            var hw = GenesisHAL.Hardware;
            var result = new Dictionary<string, GGValue>(StringComparer.Ordinal)
            {
                ["gpu_name"] = GGValue.FromString(new GGString(hw.GpuName ?? "")),
                ["vram_mb"] = GGValue.FromInt(hw.VramMb),
                ["graphics_backend"] = GGValue.FromString(new GGString(hw.GraphicsBackend.ToString())),
                ["cpu_cores"] = GGValue.FromInt(hw.CpuCores),
                ["system_memory_mb"] = GGValue.FromInt(hw.SystemMemoryMb),
                ["max_texture_size"] = GGValue.FromInt(hw.MaxTextureSize),
                ["display_width"] = GGValue.FromInt(hw.DisplayWidth),
                ["display_height"] = GGValue.FromInt(hw.DisplayHeight),
                ["refresh_rate"] = GGValue.FromInt(hw.RefreshRate)
            };

            return GGValue.FromNativeObject(result);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 2, "hal_use_Renderer", 0, (vm, args) =>
        {
            return ResolveHalCapability<IRenderer>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 3, "hal_use_AudioDevice", 0, (vm, args) =>
        {
            return ResolveHalCapability<IAudioDevice>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 4, "hal_use_InputManager", 0, (vm, args) =>
        {
            return ResolveHalCapability<IInputManager>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 5, "hal_use_FileSystem", 0, (vm, args) =>
        {
            return ResolveHalCapability<IFileSystem>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 6, "hal_use_HardwareInfo", 0, (vm, args) =>
        {
            return ResolveHalCapability<IHardwareInfo>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 7, "hal_use_EntityWorld", 0, (vm, args) =>
        {
            return ResolveHalCapability<IEntityWorld>();
        }));
    }

    #endregion

    #region HAL.hardware 字段原生函数

    private void RegisterHardwareFieldFunctions()
    {
        _registry.Register(new HalPalNativeFunction(BaseId + 20, "hal_hardware_gpu_name", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.FromString(new GGString(""));
            }

            return GGValue.FromString(new GGString(GenesisHAL.Hardware.GpuName ?? ""));
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 21, "hal_hardware_vram_mb", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt(GenesisHAL.Hardware.VramMb);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 22, "hal_hardware_graphics_backend", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.FromString(new GGString("Unknown"));
            }

            return GGValue.FromString(new GGString(GenesisHAL.Hardware.GraphicsBackend.ToString()));
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 23, "hal_hardware_cpu_cores", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt(GenesisHAL.Hardware.CpuCores);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 24, "hal_hardware_system_memory_mb", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt(GenesisHAL.Hardware.SystemMemoryMb);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 25, "hal_hardware_max_texture_size", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt(GenesisHAL.Hardware.MaxTextureSize);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 26, "hal_hardware_display_width", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt(GenesisHAL.Hardware.DisplayWidth);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 27, "hal_hardware_display_height", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt(GenesisHAL.Hardware.DisplayHeight);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 28, "hal_hardware_refresh_rate", 0, (vm, args) =>
        {
            if (!GenesisHAL.IsInitialized)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt(GenesisHAL.Hardware.RefreshRate);
        }));
    }

    #endregion

    #region PAL 原生函数

    private void RegisterPalFunctions()
    {
        _registry.Register(new HalPalNativeFunction(BaseId + 100, "pal_platform", 0, (vm, args) =>
        {
            var platform = PlatformInfo.FromCurrent();
            var result = new Dictionary<string, GGValue>(StringComparer.Ordinal)
            {
                ["os"] = GGValue.FromString(new GGString(platform.OS.ToString())),
                ["isa"] = GGValue.FromString(new GGString(platform.ISA.ToString())),
                ["channel"] = GGValue.FromString(new GGString(platform.Channel.ToString())),
                ["is_desktop"] = GGValue.FromBool(platform.IsDesktop),
                ["is_mobile"] = GGValue.FromBool(platform.IsMobile),
                ["is_web"] = GGValue.FromBool(platform.IsWeb),
                ["is_console"] = GGValue.FromBool(platform.IsConsole),
                ["is_steam"] = GGValue.FromBool(platform.IsSteam),
                ["is_wechat"] = GGValue.FromBool(platform.IsWeChat)
            };

            return GGValue.FromNativeObject(result);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 101, "pal_use_LoginService", 0, (vm, args) =>
        {
            return ResolvePalCapability<ILoginService>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 102, "pal_use_ShareService", 0, (vm, args) =>
        {
            return ResolvePalCapability<IShareService>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 103, "pal_use_PaymentService", 0, (vm, args) =>
        {
            return ResolvePalCapability<IPaymentService>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 104, "pal_use_HttpService", 0, (vm, args) =>
        {
            return ResolvePalCapability<IHttpService>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 105, "pal_use_CloudSaveService", 0, (vm, args) =>
        {
            return ResolvePalCapability<ICloudSaveService>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 106, "pal_use_AdService", 0, (vm, args) =>
        {
            return ResolvePalCapability<IAdService>();
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 107, "pal_use_AchievementService", 0, (vm, args) =>
        {
            return ResolvePalCapability<IAchievementService>();
        }));
    }

    #endregion

    #region PAL.platform 字段原生函数

    private void RegisterPlatformFieldFunctions()
    {
        _registry.Register(new HalPalNativeFunction(BaseId + 120, "pal_platform_os", 0, (vm, args) =>
        {
            return GGValue.FromString(new GGString(PlatformInfo.FromCurrent().OS.ToString()));
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 121, "pal_platform_isa", 0, (vm, args) =>
        {
            return GGValue.FromString(new GGString(PlatformInfo.FromCurrent().ISA.ToString()));
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 122, "pal_platform_channel", 0, (vm, args) =>
        {
            return GGValue.FromString(new GGString(PlatformInfo.FromCurrent().Channel.ToString()));
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 123, "pal_platform_is_desktop", 0, (vm, args) =>
        {
            return GGValue.FromBool(PlatformInfo.FromCurrent().IsDesktop);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 124, "pal_platform_is_mobile", 0, (vm, args) =>
        {
            return GGValue.FromBool(PlatformInfo.FromCurrent().IsMobile);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 125, "pal_platform_is_web", 0, (vm, args) =>
        {
            return GGValue.FromBool(PlatformInfo.FromCurrent().IsWeb);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 126, "pal_platform_is_console", 0, (vm, args) =>
        {
            return GGValue.FromBool(PlatformInfo.FromCurrent().IsConsole);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 127, "pal_platform_is_steam", 0, (vm, args) =>
        {
            return GGValue.FromBool(PlatformInfo.FromCurrent().IsSteam);
        }));

        _registry.Register(new HalPalNativeFunction(BaseId + 128, "pal_platform_is_wechat", 0, (vm, args) =>
        {
            return GGValue.FromBool(PlatformInfo.FromCurrent().IsWeChat);
        }));
    }

    #endregion

    #region 辅助方法

    private static GGValue ResolveHalCapability<T>() where T : class
    {
        if (!GenesisHAL.IsInitialized)
        {
            return GGValue.Null;
        }

        var capability = GenesisHAL.TryUse<T>();
        return capability is not null
            ? GGValue.FromNativeObject(capability)
            : GGValue.Null;
    }

    private static GGValue ResolvePalCapability<T>() where T : class
    {
        if (!GenesisHAL.IsInitialized)
        {
            return GGValue.Null;
        }

        var service = GenesisHAL.Capabilities.Get<T>();
        return service is not null
            ? GGValue.FromNativeObject(service)
            : GGValue.Null;
    }

    #endregion

    #region 内部类型

    private sealed class HalPalNativeFunction : INativeFunction
    {
        private readonly Func<IVMState, GGValue[], GGValue> _implementation;

        public int Id { get; }
        public string Name { get; }
        public int ParameterCount { get; }

        public HalPalNativeFunction(int id, string name, int parameterCount, Func<IVMState, GGValue[], GGValue> implementation)
        {
            Id = id;
            Name = name;
            ParameterCount = parameterCount;
            _implementation = implementation;
        }

        public GGValue Execute(IVMState vm, GGValue[] args)
        {
            return _implementation(vm, args);
        }
    }

    #endregion
}

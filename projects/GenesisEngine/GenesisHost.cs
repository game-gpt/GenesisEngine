using Genesis.Integration;
using Genesis.Integration.Audio;
using Genesis.Integration.Navigation;
using Genesis.Integration.Physics;
using Genesis.Integration.Rendering;
using Genesis.Runtime;
using Gnosis.ECS.World;
using Gnosis.Graphic.Pipeline;
using Gnosis.Graphic.RHI;
using Gnosis.Graphic.RHI.OpenGL;
using Gnosis.Graphic.Sprite2D;
using Gnosis.Graphic.Texture;
using Gnosis.Graphic.Window;
using Gnosis.Input.Device;
using Gnosis.Input.Simulate;

namespace GenesisEngine;

public class GenesisHost : IDisposable
{
    #region 私有字段

    private readonly ulong _worldSeed;
    private World? _world;
    private bool _isRunning;
    private bool _disposed;
    private DateTime _lastFrameTime;
    private ulong _frameIndex;
    private GraphicsBackend _activeBackend;
    private OpenGLDevice? _glDevice;

    #endregion

    #region 2D 子系统集成（M3 泰拉瑞亚）

    private Graphic2DIntegration? _graphic2D;
    private Physics2DIntegration? _physics2D;
    private AudioIntegration? _audio2D;
    private AIIntegration? _ai2D;
    private NavigationIntegration? _navigation2D;

    #endregion

    #region 3D 子系统集成（M4 我的世界）

    private GenesisRenderSystem? _render3D;
    private GenesisPhysicsSystem? _physics3D;
    private GenesisAudioSystem? _audio3D;
    private GenesisNavigationSystem? _navigation3D;

    #endregion

    #region 窗口与渲染

    private IWindow? _window;
    private ForwardRenderer? _renderer;
    private Camera2D? _camera2D;
    private Sprite2DRenderPass? _sprite2DPass;
    private TextureLoader? _textureLoader;
    private InputSystem? _inputSystem;

    #endregion

    #region 脚本运行时

    private ScriptRuntime? _scriptRuntime;

    #endregion

    #region 属性 - 2D 子系统

    public Graphic2DIntegration Graphic2D => _graphic2D ?? throw new InvalidOperationException("引擎未初始化");

    public Physics2DIntegration Physics2D => _physics2D ?? throw new InvalidOperationException("引擎未初始化");

    public AudioIntegration Audio2D => _audio2D ?? throw new InvalidOperationException("引擎未初始化");

    public AIIntegration AI2D => _ai2D ?? throw new InvalidOperationException("引擎未初始化");

    public NavigationIntegration Navigation2D => _navigation2D ?? throw new InvalidOperationException("引擎未初始化");

    #endregion

    #region 属性 - 3D 子系统

    public GenesisRenderSystem Render3D => _render3D ?? throw new InvalidOperationException("引擎未初始化");

    public GenesisPhysicsSystem Physics3D => _physics3D ?? throw new InvalidOperationException("引擎未初始化");

    public GenesisAudioSystem Audio3D => _audio3D ?? throw new InvalidOperationException("引擎未初始化");

    public GenesisNavigationSystem Navigation3D => _navigation3D ?? throw new InvalidOperationException("引擎未初始化");

    #endregion

    #region 属性 - 窗口与渲染

    public IWindow? Window => _window;
    public ForwardRenderer? Renderer => _renderer;
    public Camera2D? Camera2D => _camera2D;
    public InputSystem? InputSystem => _inputSystem;
    public ScriptRuntime? ScriptRuntime => _scriptRuntime;

    #endregion

    #region 构造函数

    public GenesisHost(ulong worldSeed)
    {
        _worldSeed = worldSeed;
    }

    #endregion

    #region 公开方法

    public void Initialize()
    {
        Console.WriteLine($"[Genesis] 引擎初始化 - 世界种子: {_worldSeed}");

        _world = new World();

        Initialize2DSystems();
        Initialize3DSystems();

        _isRunning = true;
        _lastFrameTime = DateTime.UtcNow;

        Console.WriteLine("[Genesis] 所有子系统初始化完成");
        Console.WriteLine("[Genesis]   - 2D 渲染: Gnosis.Graphic 2D");
        Console.WriteLine("[Genesis]   - 2D 物理: Gnosis.Physics 2D");
        Console.WriteLine("[Genesis]   - 2D 音频: Gnosis.Audio");
        Console.WriteLine("[Genesis]   - 2D AI:   Gnosis.AI 行为树");
        Console.WriteLine("[Genesis]   - 2D 导航: Gnosis.Navigation");
        Console.WriteLine("[Genesis]   - 3D 渲染: Gnosis.Graphic ForwardRenderer + VoxelRenderer");
        Console.WriteLine("[Genesis]   - 3D 物理: Gnosis.Physics 3D 刚体/碰撞");
        Console.WriteLine("[Genesis]   - 3D 音频: Gnosis.Audio 3D 空间化");
        Console.WriteLine("[Genesis]   - 3D 寻路: Gnosis.Navigation NavMesh + A*");
    }

    public void InitializeWithWindow(uint width = 1280, uint height = 720, string title = "Gnosis Engine", GraphicsBackend backend = GraphicsBackend.Vulkan)
    {
        Console.WriteLine($"[Genesis] 引擎初始化（窗口模式）- 世界种子: {_worldSeed}");

        _world = new World();
        _inputSystem = new InputSystem();

        var actualBackend = DetermineBackend(backend);
        _activeBackend = actualBackend;

        var options = new WindowOptions
        {
            Title = title,
            Width = width,
            Height = height,
            VSync = true,
            Resizable = true
        };

        _window = SilkNetWindow.Create(options);

        if (_window is SilkNetWindow silkWindow)
        {
            var keyboard = (Keyboard)_inputSystem.Keyboard!;
            var mouse = (Mouse)_inputSystem.Mouse!;
            silkWindow.SetInputDevices(keyboard, mouse);
        }

        _window.OnClosing += _ => _isRunning = false;

        _camera2D = new Camera2D
        {
            ViewportSize = new System.Numerics.Vector2(width, height)
        };

        _scriptRuntime = new ScriptRuntime();
        _scriptRuntime.Initialize(_world);

        _isRunning = true;
        _lastFrameTime = DateTime.UtcNow;

        Console.WriteLine($"[Genesis] 窗口模式初始化完成 - {width}x{height} - 后端: {actualBackend}");
    }

    public void LoadGameScript(string scriptPath)
    {
        if (_scriptRuntime is null)
        {
            throw new InvalidOperationException("请先调用 InitializeWithWindow()");
        }

        if (Directory.Exists(scriptPath))
        {
            _scriptRuntime.LoadScriptDirectory(scriptPath);
        }
        else if (File.Exists(scriptPath))
        {
            var source = File.ReadAllText(scriptPath);
            _scriptRuntime.LoadScriptSource(source, scriptPath);
        }
        else
        {
            Console.WriteLine($"[Genesis] 脚本路径不存在: {scriptPath}");
        }
    }

    public void Initialize3DRendering(nint windowHandle, uint width, uint height)
    {
        if (_render3D is null || _world is null)
        {
            throw new InvalidOperationException("引擎未初始化");
        }

        _render3D.InitializeAuto();
        Console.WriteLine($"[Genesis] 3D 渲染已初始化 - {width}x{height}");
    }

    public void Run()
    {
        if (_world is null)
        {
            throw new InvalidOperationException("引擎未初始化，请先调用 Initialize()");
        }

        if (_window is not null)
        {
            RunWithWindow();
        }
        else
        {
            RunHeadless();
        }
    }

    public void Shutdown()
    {
        _isRunning = false;
        Console.WriteLine("[Genesis] 引擎关闭");
    }

    #endregion

    #region 私有方法 - 渲染

    private void InitializeGraphicsBackend()
    {
        if (_activeBackend == GraphicsBackend.OpenGL)
        {
            var glDevice = new OpenGLDevice();
            glDevice.Initialize(NativeGetProcAddress);
            _glDevice = glDevice;

            Initialize2DSystems(_activeBackend, glDevice);

            Console.WriteLine("[Genesis] OpenGL 设备初始化完成 - 函数指针已加载");
        }
        else
        {
            var device = DeviceFactory.Create(_activeBackend);
            Initialize2DSystems(_activeBackend, device);
        }
    }

    #endregion

    #region 私有方法 - 游戏循环

    private void RunWithWindow()
    {
        Console.WriteLine("[Genesis] 窗口模式主循环启动");

        _scriptRuntime?.Run();

        _window.PollEvents();

        InitializeGraphicsBackend();

        while (_isRunning && !_window!.IsClosing)
        {
            _window.PollEvents();

            _inputSystem?.Update();

            var now = DateTime.UtcNow;
            var delta = (float)(now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;
            if (delta > 0.1f) delta = 0.1f;

            Update(delta);

            _frameIndex++;
        }

        Console.WriteLine("[Genesis] 窗口模式主循环退出");
    }

    private void RunHeadless()
    {
        Console.WriteLine("[Genesis] 无头模式主循环启动");

        while (_isRunning)
        {
            var now = DateTime.UtcNow;
            var delta = (float)(now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;

            if (delta > 0.1f)
            {
                delta = 0.1f;
            }

            Update(delta);
        }

        Console.WriteLine("[Genesis] 无头模式主循环退出");
    }

    #endregion

    #region 私有方法 - 初始化

    private static GraphicsBackend DetermineBackend(GraphicsBackend preferred)
    {
        if (preferred == GraphicsBackend.OpenGL || preferred == GraphicsBackend.Software)
        {
            return preferred;
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                var module = System.Runtime.InteropServices.NativeLibrary.TryLoad("vulkan-1.dll", out _);
                if (!module)
                {
                    Console.WriteLine("[Genesis] Vulkan 运行时不可用，回退到 OpenGL");
                    return GraphicsBackend.OpenGL;
                }
            }

            return preferred;
        }
        catch
        {
            Console.WriteLine("[Genesis] 后端检测失败，回退到 OpenGL");
            return GraphicsBackend.OpenGL;
        }
    }

    private static nint NativeGetProcAddress(string name)
    {
        if (OperatingSystem.IsWindows())
        {
            var ptr = WglGetProcAddressNative(name);
            if (ptr != 0)
            {
                return ptr;
            }
        }

        try
        {
            var lib = System.Runtime.InteropServices.NativeLibrary.Load("opengl32.dll");
            return System.Runtime.InteropServices.NativeLibrary.GetExport(lib, name);
        }
        catch
        {
            return 0;
        }
    }

    [System.Runtime.InteropServices.DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress")]
    private static extern nint WglGetProcAddressNative(string name);

    private void Initialize2DSystems(GraphicsBackend backend = default, IDevice? device = null)
    {
        _graphic2D = new Graphic2DIntegration();
        _physics2D = new Physics2DIntegration();
        _audio2D = new AudioIntegration();
        _ai2D = new AIIntegration();
        _navigation2D = new NavigationIntegration();

        if (device is not null && device is OpenGLDevice glDevice)
        {
            _graphic2D.InitializeWithDevice(glDevice);
        }
        else
        {
            _graphic2D.Initialize(backend);
        }

        _physics2D.Initialize();
        _audio2D.Initialize();
        _ai2D.Initialize();
        _navigation2D.Initialize();
    }

    private void Initialize3DSystems()
    {
        if (_world is null)
        {
            return;
        }

        _render3D = new GenesisRenderSystem();
        _physics3D = new GenesisPhysicsSystem();
        _audio3D = new GenesisAudioSystem();
        _navigation3D = new GenesisNavigationSystem();

        _world.Systems.RegisterSystem(_render3D);
        _world.Systems.RegisterSystem(_physics3D);
        _world.Systems.RegisterSystem(_audio3D);
        _world.Systems.RegisterSystem(_navigation3D);

        _physics3D.Initialize();
        _audio3D.Initialize();
        _navigation3D.Initialize();
    }

    #endregion

    #region 私有方法 - 更新

    private void Update(float delta)
    {
        _physics2D?.Update(delta);
        _ai2D?.Update(delta);
        _navigation2D?.Update(delta);
        _audio2D?.Update(delta);

        if (_scriptRuntime is not null && _scriptRuntime.IsInitialized)
        {
            _scriptRuntime.Tick(delta);
        }
        else
        {
            _world?.Update(delta);
        }

        _render3D?.Update(delta);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _scriptRuntime?.Shutdown();
        _renderer?.Shutdown();
        _window?.Dispose();

        _render3D?.Shutdown();
        _navigation3D?.Shutdown();
        _audio3D?.Shutdown();
        _physics3D?.Shutdown();

        _navigation2D?.Dispose();
        _ai2D?.Dispose();
        _audio2D?.Dispose();
        _physics2D?.Dispose();
        _graphic2D?.Dispose();

        _world = null;
        _disposed = true;
    }

    #endregion
}

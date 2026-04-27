using Genesis.Attention;
using Genesis.Core;
using Genesis.Integration.AI2D;
using Genesis.Integration.Audio;
using Genesis.Integration.Audio2D;
using Genesis.Integration.Navigation;
using Genesis.Integration.Navigation2D;
using Genesis.Integration.Physics;
using Genesis.Integration.Physics2D;
using Genesis.Integration.Rendering;
using Genesis.Integration.Rendering2D;
using Genesis.Rendering;
using Genesis.Runtime;
using GenesisEngine.Rendering;
using Genesis.Spacetime;
using Genesis.World;
using Gnosis.ECS.World;
using Gnosis.Graphic.RHI;
using Gnosis.Graphic.Sprite2D;
using Gnosis.Graphic.UI;
using Gnosis.Graphic.Window;
using Gnosis.Input.Device;
using Gnosis.Input.Simulate;
using Gnosis.Platform.Window;
using Gnosis.Platform.Window.GL;
using Gnosis.Platform.Window.Win32;
using Gnosis.Widget.Element;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;

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
    private GraphicsBackend _backend;
    private GLContext? _gl;

    #endregion

    #region 2D 子系统集成（ECS System 模式）

    private Genesis2DRenderSystem? _render2D;
    private Genesis2DPhysicsSystem? _physics2D;
    private Genesis2DAudioSystem? _audio2D;
    private Genesis2DAISystem? _ai2D;
    private Genesis2DNavigationSystem? _navigation2D;

    #endregion

    #region 3D 子系统集成（ECS System 模式）

    private GenesisRenderSystem? _render3D;
    private GenesisPhysicsSystem? _physics3D;
    private GenesisAudioSystem? _audio3D;
    private GenesisNavigationSystem? _navigation3D;

    #endregion

    #region 窗口与渲染

    private IWindow? _window;
    private Camera2D? _camera2D;
    private InputSystem? _inputSystem;

    #endregion

    #region Widget UI

    private GpuWidgetRenderer? _widgetRenderer;
    private WidgetTreeRenderer? _widgetTreeRenderer;
    private Dock? _rootWidget;
    private FocusManager? _focusManager;
    private GLUiRenderer? _uiRenderer;

    #endregion

    #region 脚本运行时

    private ScriptRuntime? _scriptRuntime;

    #endregion

    #region 世界生成

    private SpacetimeTree? _spacetimeTree;
    private SimpleAttentionManager? _attentionManager;
    private ChunkCoordinator? _chunkCoordinator;
    private SceneCollapser? _sceneCollapser;
    private PostProcessPipeline? _postProcessPipeline;

    #endregion

    #region 属性 - 2D 子系统

    public Genesis2DRenderSystem Render2D => _render2D ?? throw new InvalidOperationException("引擎未初始化");

    public Genesis2DPhysicsSystem Physics2D => _physics2D ?? throw new InvalidOperationException("引擎未初始化");

    public Genesis2DAudioSystem Audio2D => _audio2D ?? throw new InvalidOperationException("引擎未初始化");

    public Genesis2DAISystem AI2D => _ai2D ?? throw new InvalidOperationException("引擎未初始化");

    public Genesis2DNavigationSystem Navigation2D => _navigation2D ?? throw new InvalidOperationException("引擎未初始化");

    #endregion

    #region 属性 - 3D 子系统

    public GenesisRenderSystem Render3D => _render3D ?? throw new InvalidOperationException("引擎未初始化");

    public GenesisPhysicsSystem Physics3D => _physics3D ?? throw new InvalidOperationException("引擎未初始化");

    public GenesisAudioSystem Audio3D => _audio3D ?? throw new InvalidOperationException("引擎未初始化");

    public GenesisNavigationSystem Navigation3D => _navigation3D ?? throw new InvalidOperationException("引擎未初始化");

    #endregion

    #region 属性 - 窗口与渲染

    public IWindow? Window => _window;
    public Camera2D? Camera2D => _camera2D;
    public InputSystem? InputSystem => _inputSystem;
    public ScriptRuntime? ScriptRuntime => _scriptRuntime;
    public GraphicsBackend Backend => _backend;

    #endregion

    #region 属性 - 世界生成

    public ChunkCoordinator? ChunkCoordinator => _chunkCoordinator;
    public PostProcessPipeline? PostProcessPipeline => _postProcessPipeline;
    public SceneCollapser? SceneCollapser => _sceneCollapser;

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
    }

    public void InitializeWithWindow(uint width = 1280, uint height = 720, string title = "Genesis Engine", GraphicsBackend? backend = null)
    {
        _backend = backend ?? DeviceFactory.DetectBestBackend();

        Console.WriteLine($"[Genesis] 引擎初始化（窗口模式）- 世界种子: {_worldSeed} - 后端: {_backend}");

        _world = new World();
        _inputSystem = new InputSystem();

        var options = new WindowOptions
        {
            Title = title,
            Width = width,
            Height = height,
            VSync = true,
            Resizable = true
        };

        _window = PlatformWindowAdapter.Create(options);

        if (_window is PlatformWindowAdapter adapter)
        {
            var platformWindow = adapter.GetPlatformWindow();
            if (platformWindow is Win32Window win32Window)
            {
                var keyboard = (Keyboard)_inputSystem.Keyboard!;
                var mouse = (Mouse)_inputSystem.Mouse!;
                win32Window.SetInputDevices(keyboard, mouse);
            }
        }

        _window.OnClosing += _ => _isRunning = false;

        _camera2D = new Camera2D
        {
            ViewportSize = new System.Numerics.Vector2(width, height)
        };

        _scriptRuntime = new ScriptRuntime();
        _scriptRuntime.Initialize(_world);

        InitializeWorldGeneration();

        _isRunning = true;
        _lastFrameTime = DateTime.UtcNow;

        Console.WriteLine($"[Genesis] 窗口模式初始化完成 - {width}x{height} - {_backend} 后端");
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

    #region 私有方法 - 游戏循环

    private void RunWithWindow()
    {
        Console.WriteLine("[Genesis] 窗口模式主循环启动");

        _scriptRuntime?.Run();

        _window.MakeCurrent();

        if (_window.GlContext is GLContext gl)
        {
            _gl = gl;
            Console.WriteLine("[Genesis] OpenGL 上下文已获取");
        }

        Initialize2DSystems(GraphicsBackend.OpenGL);

        _window.MakeCurrent();
        InitializeWidgetUI();

        while (_isRunning && !_window!.IsClosing)
        {
            _window.PollEvents();

            _inputSystem?.Update();

            var now = DateTime.UtcNow;
            var delta = (float)(now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;
            if (delta > 0.1f) delta = 0.1f;

            Update(delta);

            if (!_window.IsMinimized)
            {
                RenderFrame();
            }

            _frameIndex++;
        }

        Console.WriteLine("[Genesis] 窗口模式主循环退出");
    }

    private void RenderFrame()
    {
        if (_gl is null)
        {
            return;
        }

        _window?.MakeCurrent();

        var t = (float)(_frameIndex * 0.016);
        var r = (float)(0.53 + 0.1 * Math.Sin(t * 0.6));
        var g = (float)(0.81 + 0.1 * Math.Sin(t * 0.4));
        var b = (float)(0.92 + 0.05 * Math.Sin(t * 0.8));

        _gl.ClearColor(r, g, b, 1.0f);
        _gl.Clear(GLContext.ColorBufferBit | GLContext.DepthBufferBit);

        RenderWidgets();

        if (_render2D is not null && _render2D.IsInitialized)
        {
            var cameraOffset = System.Numerics.Vector2.Zero;
            if (_camera2D is not null)
            {
                cameraOffset = new System.Numerics.Vector2(_camera2D.Position.X, _camera2D.Position.Y);
            }

            _render2D.BeginFrame();
            _render2D.RenderWithCamera(cameraOffset);
            _render2D.EndFrame();
        }

        _window?.SwapBuffers();
    }

    private void RenderWidgets()
    {
        if (_widgetRenderer is null || _widgetTreeRenderer is null || _rootWidget is null || _window is null)
        {
            return;
        }

        _widgetRenderer.Begin();
        _widgetTreeRenderer.Render(_rootWidget, _window.Width, _window.Height);
        _widgetRenderer.End();

        if (_uiRenderer is not null)
        {
            _uiRenderer.Render(_widgetRenderer);
        }
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

    #region 私有方法 - Widget UI

    private void InitializeWidgetUI()
    {
        if (_window is null || _gl is null)
        {
            return;
        }

        _widgetRenderer = new GpuWidgetRenderer((int)_window.Width, (int)_window.Height);
        _widgetTreeRenderer = new WidgetTreeRenderer(_widgetRenderer);

        BuildMainMenuWidget();

        _focusManager = new FocusManager(_rootWidget!);

        _uiRenderer = new GLUiRenderer(_gl);
        if (!_uiRenderer.Initialize())
        {
            Console.WriteLine("[Genesis] UI 渲染器初始化失败，Widget 将不可见");
            _uiRenderer = null;
        }

        Console.WriteLine("[Genesis] Widget UI 系统初始化完成");
    }

    private void BuildMainMenuWidget()
    {
        _rootWidget = new Dock();

        var titleBar = new VBox();
        titleBar.Margin = new EdgeInsets(12, 0, 12, 0);
        titleBar.Background = new Color(0.1f, 0.1f, 0.12f, 0.95f);

        var titleText = new TextWidget("Genesis Engine");
        titleText.Margin = new EdgeInsets(8, 16, 8, 4);
        titleText.Foreground = new Color(0.9f, 0.9f, 0.95f, 1.0f);
        titleBar.AddChild(titleText);

        var subtitleText = new TextWidget("游戏选择菜单");
        subtitleText.Margin = new EdgeInsets(8, 0, 8, 12);
        subtitleText.Foreground = new Color(0.6f, 0.6f, 0.7f, 1.0f);
        titleBar.AddChild(subtitleText);

        _rootWidget.DockWidget(titleBar, DockPosition.Top);

        var contentPanel = new VBox();
        contentPanel.Margin = new EdgeInsets(24, 12, 24, 12);
        contentPanel.Background = new Color(0.08f, 0.08f, 0.1f, 0.9f);

        var gameNames = new[]
        {
            "1. 异星工厂 (Factorio)",
            "2. 我的世界 (Minecraft)",
            "3. 女巫 (Noita)",
            "4. 小小泰拉瑞亚 (Terraria)"
        };

        foreach (var name in gameNames)
        {
            var btn = new ButtonWidget();
            btn.Margin = new EdgeInsets(8, 6, 8, 6);
            btn.Foreground = new Color(0.9f, 0.9f, 0.95f, 1.0f);
            btn.Background = new Color(0.15f, 0.15f, 0.2f, 1.0f);

            var label = new TextWidget(name);
            label.Foreground = new Color(0.9f, 0.9f, 0.95f, 1.0f);
            btn.AddChild(label);

            contentPanel.AddChild(btn);
        }

        var hint = new TextWidget("使用 --game factorio|minecraft|noita|terraria 启动对应游戏");
        hint.Margin = new EdgeInsets(8, 16, 8, 8);
        hint.Foreground = new Color(0.4f, 0.4f, 0.5f, 1.0f);
        contentPanel.AddChild(hint);

        _rootWidget.DockWidget(contentPanel, DockPosition.Fill);
    }

    #endregion

    #region 私有方法 - 初始化

    private void Initialize2DSystems(GraphicsBackend backend = default)
    {
        if (_world is null)
        {
            return;
        }

        _render2D = new Genesis2DRenderSystem();
        _physics2D = new Genesis2DPhysicsSystem();
        _audio2D = new Genesis2DAudioSystem();
        _ai2D = new Genesis2DAISystem();
        _navigation2D = new Genesis2DNavigationSystem();

        _world.Systems.RegisterSystem(_render2D);
        _world.Systems.RegisterSystem(_physics2D);
        _world.Systems.RegisterSystem(_audio2D);
        _world.Systems.RegisterSystem(_ai2D);
        _world.Systems.RegisterSystem(_navigation2D);

        try
        {
            _render2D.InitializeWithBackend(backend == default ? GraphicsBackend.OpenGL : backend);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Genesis] 2D 渲染初始化失败（非致命）: {ex.Message}");
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
        if (_scriptRuntime is not null && _scriptRuntime.IsInitialized)
        {
            _scriptRuntime.Tick(delta);
        }
        else
        {
            _world?.Update(delta);
        }

        UpdateWorldGeneration(delta);
    }

    #endregion

    #region 私有方法 - 世界生成

    private void InitializeWorldGeneration()
    {
        _spacetimeTree = new SpacetimeTree();
        var attentionModel = new SimpleAttentionModel();
        _attentionManager = new SimpleAttentionManager(_spacetimeTree, attentionModel);

        var noiseGenerator = new SimpleNoiseGenerator(_worldSeed);
        var wfcGenerator = new SimpleWFCGenerator();
        var worldGenerator = new SimpleWorldGenerator(noiseGenerator, wfcGenerator);

        _chunkCoordinator = new ChunkCoordinator(
            worldGenerator,
            noiseGenerator,
            _spacetimeTree,
            _worldSeed,
            chunkSize: 32,
            loadRadius: 4,
            unloadRadius: 6,
            attentionManager: _attentionManager);

        _sceneCollapser = new SceneCollapser(_spacetimeTree, _attentionManager);
        _postProcessPipeline = new PostProcessPipeline();
        _postProcessPipeline.ApplyPreset(PostProcessPreset.Default);

        Console.WriteLine("[Genesis] 世界生成系统初始化完成 - 区块大小 32, 加载半径 4");
    }

    private void UpdateWorldGeneration(float delta)
    {
        if (_chunkCoordinator is null || _camera2D is null)
        {
            return;
        }

        var playerPos = new Position(_camera2D.Position.X, 0, _camera2D.Position.Y);
        _chunkCoordinator.Update(playerPos);
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
        _uiRenderer?.Dispose();
        _window?.Dispose();

        _navigation2D?.Shutdown();
        _ai2D?.Shutdown();
        _audio2D?.Shutdown();
        _physics2D?.Shutdown();
        _render2D?.Shutdown();

        _navigation3D?.Shutdown();
        _audio3D?.Shutdown();
        _physics3D?.Shutdown();
        _render3D?.Shutdown();

        _world = null;
        _disposed = true;
    }

    #endregion
}

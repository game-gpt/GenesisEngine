using Genesis.Attention;
using Genesis.Causal;
using Genesis.Core;
using Genesis.GameSystems;
using Genesis.HAL;
using Genesis.HAL.Adapters;
using Genesis.Rendering;
using Genesis.Rules;
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
using Gnosis.PAL;
using Gnosis.Platform.Window;
using Gnosis.Platform.Window.GL;
using Gnosis.Platform.Window.Win32;
using Gnosis.Widget.Element;
using WidgetMouseButton = Gnosis.Widget.Element.MouseButton;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;
using GraphicsBackend = Gnosis.Graphic.RHI.GraphicsBackend;

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

    #region 统一游戏系统（E3/E4 合并后）

    private RenderGameSystem? _renderSystem;
    private PhysicsGameSystem? _physicsSystem;
    private AudioGameSystem? _audioSystem;
    private AIGameSystem? _aiSystem;
    private NavigationGameSystem? _navigationSystem;

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

    #region 属性 - 游戏系统

    public RenderGameSystem RenderSystem => _renderSystem ?? throw new InvalidOperationException("引擎未初始化");

    public PhysicsGameSystem PhysicsSystem => _physicsSystem ?? throw new InvalidOperationException("引擎未初始化");

    public AudioGameSystem AudioSystem => _audioSystem ?? throw new InvalidOperationException("引擎未初始化");

    public AIGameSystem AISystem => _aiSystem ?? throw new InvalidOperationException("引擎未初始化");

    public NavigationGameSystem NavigationSystem => _navigationSystem ?? throw new InvalidOperationException("引擎未初始化");

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

        InitializeGameSystems();

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
        if (_renderSystem is null || _world is null)
        {
            throw new InvalidOperationException("引擎未初始化");
        }

        _renderSystem.InitializeAuto();
        Console.WriteLine($"[Genesis] 渲染已初始化 - {width}x{height}");
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

        Initialize2DGameSystems(GraphicsBackend.OpenGL);

        _window.MakeCurrent();
        InitializeWidgetUI();

        while (_isRunning && !_window!.IsClosing)
        {
            _window.PollEvents();

            _inputSystem?.Update();

            RouteInputToWidgets();

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

        _gl.Viewport(0, 0, (int)_window.Width, (int)_window.Height);

        var t = (float)(_frameIndex * 0.016);
        var r = (float)(0.53 + 0.1 * Math.Sin(t * 0.6));
        var g = (float)(0.81 + 0.1 * Math.Sin(t * 0.4));
        var b = (float)(0.92 + 0.05 * Math.Sin(t * 0.8));

        _gl.ClearColor(r, g, b, 1.0f);
        _gl.Clear(GLContext.ColorBufferBit | GLContext.DepthBufferBit);

        if (_renderSystem is not null && _renderSystem.IsInitialized)
        {
            var cameraOffset = System.Numerics.Vector2.Zero;
            if (_camera2D is not null)
            {
                cameraOffset = new System.Numerics.Vector2(_camera2D.Position.X, _camera2D.Position.Y);
            }

            _renderSystem.BeginFrame();
            _renderSystem.RenderWithCamera(cameraOffset);
            _renderSystem.EndFrame();
        }

        RenderWidgets();

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

    private void RouteInputToWidgets()
    {
        if (_inputSystem is null || _rootWidget is null || _window is null)
        {
            return;
        }

        var mouse = _inputSystem.Mouse;
        if (mouse is not null)
        {
            var pos = mouse.Position;
            var leftDown = mouse.GetButtonDown(0);
            var leftUp = mouse.GetButtonUp(0);

            if (leftDown)
            {
                var target = HitTest(_rootWidget, pos.X, pos.Y);
                if (target is not null)
                {
                    if (target.IsFocusable)
                    {
                        _focusManager?.SetFocus(target);
                    }

                    var args = new MouseEventArgs(pos.X, pos.Y, WidgetMouseButton.Left);
                    target.DispatchEvent(args);
                }
            }
            else if (leftUp)
            {
                var target = HitTest(_rootWidget, pos.X, pos.Y);
                if (target is not null)
                {
                    var args = new MouseEventArgs(pos.X, pos.Y, WidgetMouseButton.Left);
                    target.DispatchEvent(args);
                }
            }

            var scrollDelta = mouse.ScrollDelta;
            if (Math.Abs(scrollDelta) > 0.001f)
            {
                var target = HitTest(_rootWidget, pos.X, pos.Y);
                if (target is not null)
                {
                    var args = new WheelEventArgs(pos.X, pos.Y, scrollDelta);
                    target.DispatchEvent(args);
                }
            }
        }

        var keyboard = _inputSystem.Keyboard;
        if (keyboard is not null && _focusManager?.FocusedElement is not null)
        {
            for (int i = 0; i < 256; i++)
            {
                if (keyboard.GetKeyDown(i))
                {
                    var key = MapKeyCodeToKey(i);
                    var args = new KeyEventArgs(key);
                    _focusManager.FocusedElement.DispatchEvent(args);
                }
            }
        }
    }

    private static Key MapKeyCodeToKey(int keyCode)
    {
        return keyCode switch
        {
            0x08 => Key.Back,
            0x09 => Key.Tab,
            0x0D => Key.Enter,
            0x1B => Key.Escape,
            0x20 => Key.Space,
            0x25 => Key.Left,
            0x26 => Key.Up,
            0x27 => Key.Right,
            0x28 => Key.Down,
            0x2E => Key.Delete,
            >= 0x30 and <= 0x39 => Key.D0 + (keyCode - 0x30),
            >= 0x41 and <= 0x5A => Key.A + (keyCode - 0x41),
            >= 0x70 and <= 0x87 => Key.F1 + (keyCode - 0x70),
            _ => Key.None
        };
    }

    private static WidgetElement? HitTest(WidgetElement widget, float x, float y)
    {
        if (widget.Visibility != Visibility.Visible)
        {
            return null;
        }

        if (!widget.LayoutRect.Contains(x, y))
        {
            return null;
        }

        if (widget is ContainerElement container)
        {
            for (var i = container.Children.Count - 1; i >= 0; i--)
            {
                var child = container.Children[i];
                var hit = HitTest(child, x, y);
                if (hit is not null)
                {
                    return hit;
                }
            }
        }

        return widget;
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

    private void InitializeGameSystems()
    {
        if (_world is null)
        {
            return;
        }

        InitializeHAL();

        _renderSystem = new RenderGameSystem();
        _physicsSystem = new PhysicsGameSystem();
        _audioSystem = new AudioGameSystem();
        _aiSystem = new AIGameSystem();
        _navigationSystem = new NavigationGameSystem();

        _world.Systems.RegisterSystem(_renderSystem);
        _world.Systems.RegisterSystem(_physicsSystem);
        _world.Systems.RegisterSystem(_audioSystem);
        _world.Systems.RegisterSystem(_aiSystem);
        _world.Systems.RegisterSystem(_navigationSystem);

        if (_entityWorldAdapter is not null)
        {
            _renderSystem.SetEntityWorld(_entityWorldAdapter);
            _audioSystem.SetEntityWorld(_entityWorldAdapter);
            _physicsSystem.SetEntityWorld(_entityWorldAdapter);
            _navigationSystem.SetEntityWorld(_entityWorldAdapter);
            _aiSystem.SetEntityWorld(_entityWorldAdapter);
        }

        try { _physicsSystem.Initialize(); }
        catch (Exception ex) { Console.WriteLine($"[Genesis] 物理初始化失败（非致命）: {ex.Message}"); }

        try { _audioSystem.Initialize(); }
        catch (Exception ex) { Console.WriteLine($"[Genesis] 音频初始化失败（非致命）: {ex.Message}"); }

        try { _aiSystem.Initialize(); }
        catch (Exception ex) { Console.WriteLine($"[Genesis] AI 初始化失败（非致命）: {ex.Message}"); }

        try { _navigationSystem.Initialize(); }
        catch (Exception ex) { Console.WriteLine($"[Genesis] 导航初始化失败（非致命）: {ex.Message}"); }
    }

    private GnosisEntityWorldAdapter? _entityWorldAdapter;

    private void InitializeHAL()
    {
        if (_world is null)
        {
            return;
        }

        _entityWorldAdapter = new GnosisEntityWorldAdapter(_world);

        GenesisHAL.RegisterEntityWorld(_entityWorldAdapter);

        var hwInfo = GnosisHardwareInfoAdapter.Detect();
        GenesisHAL.RegisterHardware(hwInfo);

        GenesisHAL.RegisterFileSystem(new DefaultFileSystem());

        if (_inputSystem is not null)
        {
            GenesisHAL.RegisterInputManager(new InputSystemAdapter(_inputSystem));
        }

        var capabilityBus = new CapabilityBus(PlatformInfo.FromCurrent());
        GenesisHAL.RegisterCapabilities(capabilityBus);

        GenesisHAL.MarkInitialized();

        Console.WriteLine($"[Genesis] HAL 初始化完成 - GPU: {hwInfo.GpuName}, 后端: {hwInfo.GraphicsBackend}, CPU 核心: {hwInfo.CpuCores}");
    }

    private void Initialize2DGameSystems(GraphicsBackend backend = default)
    {
        if (_world is null)
        {
            return;
        }

        if (_renderSystem is null)
        {
            _renderSystem = new RenderGameSystem();
            _world.Systems.RegisterSystem(_renderSystem);
        }

        try
        {
            _renderSystem.InitializeWithBackend(backend == default ? GraphicsBackend.OpenGL : backend);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Genesis] 渲染初始化失败（非致命）: {ex.Message}");
        }
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

        BindEmergentEngineToScriptRuntime();

        Console.WriteLine("[Genesis] 世界生成系统初始化完成 - 区块大小 32, 加载半径 4");
    }

    private void BindEmergentEngineToScriptRuntime()
    {
        if (_scriptRuntime is null || !_scriptRuntime.IsInitialized)
        {
            return;
        }

        if (_spacetimeTree is not null)
        {
            _scriptRuntime.BindSpacetimeTree(_spacetimeTree);
        }

        if (_attentionManager is not null)
        {
            _scriptRuntime.BindAttentionManager(_attentionManager);
        }

        var causalGraph = new CausalGraph();
        _scriptRuntime.BindCausalGraph(causalGraph);

        var ruleEngine = new SimpleRuleEngine();
        _scriptRuntime.BindRuleEngine(ruleEngine);

        Console.WriteLine("[Genesis] 涌现叙事引擎已绑定到脚本运行时");
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

        _navigationSystem?.Shutdown();
        _aiSystem?.Shutdown();
        _audioSystem?.Shutdown();
        _physicsSystem?.Shutdown();
        _renderSystem?.Shutdown();

        _world = null;
        _disposed = true;
    }

    #endregion
}

using Genesis.Integration;
using Genesis.Integration.Audio;
using Genesis.Integration.Navigation;
using Genesis.Integration.Physics;
using Genesis.Integration.Rendering;
using Gnosis.ECS.World;

namespace GenesisHost;

public class GenesisHost : IDisposable
{
    #region 私有字段

    private readonly ulong _worldSeed;
    private World? _world;
    private bool _isRunning;
    private bool _disposed;
    private DateTime _lastFrameTime;

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

        Console.WriteLine("[Genesis] 引擎主循环启动");

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

        Console.WriteLine("[Genesis] 引擎主循环退出");
    }

    public void Shutdown()
    {
        _isRunning = false;
        Console.WriteLine("[Genesis] 引擎关闭");
    }

    #endregion

    #region 私有方法 - 初始化

    private void Initialize2DSystems()
    {
        _graphic2D = new Graphic2DIntegration();
        _physics2D = new Physics2DIntegration();
        _audio2D = new AudioIntegration();
        _ai2D = new AIIntegration();
        _navigation2D = new NavigationIntegration();

        _graphic2D.Initialize();
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

        _world?.Update(delta);

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

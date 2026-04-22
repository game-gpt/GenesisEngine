using Genesis.Terraria.ECS;
using Genesis.Terraria.Platform;
using Genesis.Terraria.Rendering;
using Genesis.Terraria.Systems;
using Gnosis.ECS;

namespace Genesis.Terraria;

public sealed class TerrariaGame : IDisposable
{
    #region 私有字段

    private readonly EcsWorld _world;
    private readonly List<ISystem> _systems;
    private readonly ulong _seed;

    private Win32Window? _window;
    private SoftwareRenderSystem? _renderSystem;
    private bool _isRunning;
    private bool _isDisposed;

    #endregion

    #region 构造函数

    public TerrariaGame(ulong seed = 42)
    {
        _seed = seed;
        _world = new EcsWorld();
        _systems = new List<ISystem>();
    }

    #endregion

    #region 初始化

    public void Initialize()
    {
        var screenWidth = 640;
        var screenHeight = 480;
        var tilePixelSize = 8;

        _window = new Win32Window(screenWidth, screenHeight, "Genesis Engine - 泰拉瑞亚 MVP");

        var worldGenSystem = new WorldGenerationSystem(_world, _seed);
        _systems.Add(worldGenSystem);

        var dayNightSystem = new DayNightSystem(_world);
        _systems.Add(dayNightSystem);

        var movementSystem = new PlayerMovementSystem(_world, _window);
        _systems.Add(movementSystem);

        var physicsSystem = new PhysicsSystem(_world);
        _systems.Add(physicsSystem);

        var enemyAISystem = new EnemyAISystem(_world);
        _systems.Add(enemyAISystem);

        var damageSystem = new DamageSystem(_world);
        _systems.Add(damageSystem);

        var inventorySystem = new InventorySystem(_world);
        _systems.Add(inventorySystem);

        _renderSystem = new SoftwareRenderSystem(_world, screenWidth, screenHeight, tilePixelSize);
        _systems.Add(_renderSystem);

        foreach (var system in _systems)
        {
            system.Initialize();
        }

        Console.WriteLine("[Terraria] 游戏初始化完成 - Gnosis.Graphic SoftwareRenderer");
    }

    #endregion

    #region 主循环

    public void Run()
    {
        _isRunning = true;
        var lastTime = DateTime.UtcNow;

        while (_isRunning && _window != null && _window.ProcessMessages())
        {
            var now = DateTime.UtcNow;
            var delta = (float)(now - lastTime).TotalSeconds;
            lastTime = now;

            if (delta > 0.1f)
            {
                delta = 0.1f;
            }

            if (_window.IsKeyDown(Win32Window.VirtualKey.Escape))
            {
                break;
            }

            Update(delta);
            PresentFrame();

            Thread.Sleep(1);
        }
    }

    private void Update(float delta)
    {
        foreach (var system in _systems.OrderBy(s => s.Phase))
        {
            system.Update(delta);
        }
    }

    private void PresentFrame()
    {
        if (_renderSystem == null || _window == null)
        {
            return;
        }

        var frameData = _renderSystem.Renderer.GetFramebufferData();
        _window.PresentFrame(frameData);
    }

    #endregion

    #region 关闭

    public void Shutdown()
    {
        _isRunning = false;

        foreach (var system in _systems.AsEnumerable().Reverse())
        {
            system.Shutdown();
        }

        _window?.Dispose();
        Console.WriteLine("[Terraria] 游戏已关闭");
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Shutdown();
        _isDisposed = true;
    }

    #endregion
}

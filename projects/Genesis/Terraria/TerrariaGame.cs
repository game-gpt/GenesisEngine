using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Genesis.Terraria.Systems;

namespace Genesis.Terraria;

public sealed class TerrariaGame : IDisposable
{
    private readonly EcsWorld _world;
    private readonly List<Gnosis.ECS.ISystem> _systems;
    private bool _isRunning;
    private bool _isDisposed;
    private readonly ulong _seed;

    public TerrariaGame(ulong seed = 42)
    {
        _seed = seed;
        _world = new EcsWorld();
        _systems = new List<Gnosis.ECS.ISystem>();
    }

    public void Initialize()
    {
        var worldGenSystem = new WorldGenerationSystem(_world, _seed);
        _systems.Add(worldGenSystem);

        var dayNightSystem = new DayNightSystem(_world);
        _systems.Add(dayNightSystem);

        var movementSystem = new PlayerMovementSystem(_world);
        _systems.Add(movementSystem);

        var physicsSystem = new PhysicsSystem(_world);
        _systems.Add(physicsSystem);

        var enemyAISystem = new EnemyAISystem(_world);
        _systems.Add(enemyAISystem);

        var damageSystem = new DamageSystem(_world);
        _systems.Add(damageSystem);

        var inventorySystem = new InventorySystem(_world);
        _systems.Add(inventorySystem);

        var renderSystem = new ConsoleRenderSystem(_world);
        _systems.Add(renderSystem);

        foreach (var system in _systems)
        {
            system.Initialize();
        }

        Console.WriteLine("[Terraria] 游戏初始化完成");
        Console.WriteLine("[Terraria] 操作说明:");
        Console.WriteLine("  WASD / 方向键 - 移动");
        Console.WriteLine("  空格 - 跳跃");
        Console.WriteLine("  J - 向左挖掘");
        Console.WriteLine("  K - 向右挖掘");
        Console.WriteLine("  I - 向上挖掘");
        Console.WriteLine("  S / 下箭头 - 向下挖掘");
        Console.WriteLine("  E - 攻击附近敌人");
        Console.WriteLine("  1-0 - 选择物品栏");
        Console.WriteLine("  Q - 退出游戏");
        Console.WriteLine();
    }

    public void Run()
    {
        _isRunning = true;

        var lastTime = DateTime.UtcNow;

        while (_isRunning)
        {
            var now = DateTime.UtcNow;
            var delta = (float)(now - lastTime).TotalSeconds;
            lastTime = now;

            if (delta > 0.1f)
            {
                delta = 0.1f;
            }

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q)
                {
                    _isRunning = false;
                    break;
                }
            }

            Update(delta);

            Thread.Sleep(16);
        }
    }

    private void Update(float delta)
    {
        foreach (var system in _systems.OrderBy(s => s.Phase))
        {
            system.Update(delta);
        }
    }

    public void Shutdown()
    {
        _isRunning = false;

        foreach (var system in _systems.AsEnumerable().Reverse())
        {
            system.Shutdown();
        }

        Console.WriteLine("[Terraria] 游戏已关闭");
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Shutdown();
        _isDisposed = true;
    }
}

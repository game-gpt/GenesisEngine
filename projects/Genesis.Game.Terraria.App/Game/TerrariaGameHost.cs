using Genesis.Game.Terraria.Game.Systems;
using ECSWorld = Gnosis.ECS.World.World;
using Gnosis.Input.Simulate;

namespace Genesis.Game.Terraria.Game;

public sealed class TerrariaGameHost
{
    private readonly ulong _worldSeed;
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    private readonly int _worldWidth = 800;
    private readonly int _worldHeight = 400;

    private ECSWorld _world = null!;
    private TileMap _tileMap = null!;
    private InputSystem _inputSystem = null!;
    private bool _initialized;

    public ECSWorld World => _world;
    public TileMap TileMap => _tileMap;
    public InputSystem InputSystem => _inputSystem;
    public ulong WorldSeed => _worldSeed;

    public TerrariaGameHost(ulong worldSeed, int screenWidth, int screenHeight)
    {
        _worldSeed = worldSeed;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public void Initialize()
    {
        _world = new ECSWorld();
        _tileMap = new TileMap(_worldWidth, _worldHeight);
        _inputSystem = new InputSystem();

        _world.Systems.RegisterSystem(new WorldGenerationSystem(_worldSeed, _worldWidth, _worldHeight, _tileMap));
        _world.Systems.RegisterSystem(new PlayerMovementSystem(_tileMap, _inputSystem));
        _world.Systems.RegisterSystem(new PhysicsSystem(_tileMap));
        _world.Systems.RegisterSystem(new EnemyAISystem(_tileMap));
        _world.Systems.RegisterSystem(new DayNightSystem());
        _world.Systems.RegisterSystem(new DamageSystem());

        _initialized = true;

        Console.WriteLine($"[Terraria] 游戏初始化完成 - 种子: {_worldSeed}, 世界: {_worldWidth}x{_worldHeight}");
    }

    public void Update(float delta)
    {
        if (!_initialized)
        {
            return;
        }

        _world.Update(delta);
    }

    public void Shutdown()
    {
        _initialized = false;
        Console.WriteLine("[Terraria] 游戏关闭");
    }
}

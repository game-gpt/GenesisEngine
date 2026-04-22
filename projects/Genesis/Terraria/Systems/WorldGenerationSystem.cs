using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Genesis.Terraria.World;
using Gnosis.ECS;

namespace Genesis.Terraria.Systems;

public sealed class WorldGenerationSystem : ISystem
{
    private readonly EcsWorld _world;
    private readonly ulong _seed;
    private EntityId _tileMapEntity;
    private bool _generated;

    public SystemPhase Phase => SystemPhase.Initialization;

    public EntityId TileMapEntity => _tileMapEntity;

    public WorldGenerationSystem(EcsWorld world, ulong seed)
    {
        _world = world;
        _seed = seed;
    }

    public void Initialize()
    {
        if (_generated)
        {
            return;
        }

        var width = 200;
        var height = 80;

        _tileMapEntity = _world.CreateEntity();

        var tileMap = new TileMapData(width, height, _seed);
        var generator = new TerrariaWorldGenerator(_seed);
        generator.Generate(tileMap);

        _world.AddComponent(_tileMapEntity, tileMap);

        var spawnY = generator.FindSpawnPoint(tileMap);
        var spawnX = width / 2;

        CreatePlayer(spawnX, spawnY);
        CreateEnemies(tileMap);

        _generated = true;

        Console.WriteLine($"[Terraria] 世界生成完成 - 种子: {_seed}, 尺寸: {width}x{height}");
        Console.WriteLine($"[Terraria] 玩家出生点: ({spawnX}, {spawnY})");
    }

    private void CreatePlayer(int spawnX, int spawnY)
    {
        var player = _world.CreateEntity();
        _world.AddComponent(player, new PlayerTag(0));
        _world.AddComponent(player, new TilePosition(spawnX, spawnY));
        _world.AddComponent(player, new Velocity(0, 0));
        _world.AddComponent(player, new Health(100, 100));
        _world.AddComponent(player, new CameraFollow(0, -5, 0.1f));

        var inventory = new Inventory(10, 0);
        inventory.ItemIds[0] = ItemId.CopperPickaxe;
        inventory.ItemCounts[0] = 1;
        inventory.ItemIds[1] = ItemId.CopperSword;
        inventory.ItemCounts[1] = 1;
        _world.AddComponent(player, inventory);

        Console.WriteLine($"[Terraria] 玩家实体已创建: {player}");
    }

    private void CreateEnemies(TileMapData tileMap)
    {
        var rng = new Random((int)(_seed ^ 0xCAFEBABE));
        var enemyCount = 5;

        for (var i = 0; i < enemyCount; i++)
        {
            var enemyType = rng.Next(0, 3);
            var x = rng.Next(10, tileMap.Width - 10);

            var surfaceY = FindSurfaceAt(tileMap, x);
            if (surfaceY < 0)
            {
                continue;
            }

            var enemy = _world.CreateEntity();
            _world.AddComponent(enemy, new EnemyTag(enemyType, AiState.Idle));
            _world.AddComponent(enemy, new TilePosition(x, surfaceY - 1));
            _world.AddComponent(enemy, new Velocity(0, 0));
            _world.AddComponent(enemy, new Health(
                EnemyType.GetMaxHealth(enemyType),
                EnemyType.GetMaxHealth(enemyType)));
        }
    }

    private static int FindSurfaceAt(TileMapData tileMap, int x)
    {
        for (var y = 0; y < tileMap.Height - 1; y++)
        {
            if (tileMap.GetTile(x, y) == TileId.Air && tileMap.GetTile(x, y + 1) != TileId.Air)
            {
                return y + 1;
            }
        }

        return -1;
    }

    public void Update(float delta)
    {
    }

    public void Shutdown()
    {
    }
}

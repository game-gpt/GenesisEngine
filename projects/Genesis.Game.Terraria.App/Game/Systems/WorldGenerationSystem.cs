using Gnosis.Core.Entity;
using Gnosis.ECS.Query;
using Gnosis.ECS.System;
using Gnosis.ECS.World;

namespace Genesis.Game.Terraria.Game.Systems;

public sealed class WorldGenerationSystem : IWorldSystem
{
    private World _world = null!;
    private TileMap _tileMap = null!;
    private bool _generated;
    private readonly ulong _seed;
    private readonly int _worldWidth;
    private readonly int _worldHeight;

    public SystemPhase Phase => SystemPhase.Initialization;

    public WorldGenerationSystem(ulong seed, int width, int height, TileMap tileMap)
    {
        _seed = seed;
        _worldWidth = width;
        _worldHeight = height;
        _tileMap = tileMap;
    }

    public void SetWorld(ECSWorld world) => _world = world;
    public void Initialize() { }
    public void Shutdown() { }

    public void Update(float delta)
    {
        if (_generated)
        {
            return;
        }
        _generated = true;

        var tilemapEntity = _world.CreateEntity();
        _world.AddComponent(tilemapEntity, new TileMapData
        {
            Width = _worldWidth,
            Height = _worldHeight,
            Seed = _seed
        });

        GenerateTerrain();
        GenerateCaves();
        GenerateOres();
        GenerateTrees();

        var spawnY = FindSpawnPoint();
        var spawnX = _worldWidth / 2;

        CreatePlayer(spawnX, spawnY);
        CreateEnemies();

        var daytimeEntity = _world.CreateEntity();
        _world.AddComponent(daytimeEntity, new DayTime
        {
            TimeOfDay = 0.25f,
            DayDuration = 600.0f,
            DayCount = 1
        });

        Console.WriteLine($"[Terraria] 世界生成完成 - 种子: {_seed}, 尺寸: {_worldWidth}x{_worldHeight}");
    }

    private void GenerateTerrain()
    {
        var surfaceBase = _worldHeight * 0.3;
        var rng = new Random((int)_seed);

        for (var x = 0; x < _worldWidth; x++)
        {
            var surfaceHeight = (int)(surfaceBase + SimpleNoise(x * 0.02, _seed) * 20);
            var biome = GetBiomeAt(x);

            for (var y = 0; y < _worldHeight; y++)
            {
                if (y < surfaceHeight)
                {
                    _tileMap.SetTile(x, y, Tile.Air);
                }
                else if (y == surfaceHeight)
                {
                    _tileMap.SetTile(x, y, GetSurfaceTile(biome));
                }
                else if (y < surfaceHeight + 5)
                {
                    _tileMap.SetTile(x, y, GetSubsurfaceTile(biome));
                }
                else
                {
                    _tileMap.SetTile(x, y, Tile.Stone);
                }
            }
        }
    }

    private void GenerateCaves()
    {
        for (var x = 0; x < _worldWidth; x++)
        {
            for (var y = 0; y < _worldHeight; y++)
            {
                if (_tileMap.GetTile(x, y) == Tile.Air)
                {
                    continue;
                }
                var caveValue = SimpleNoise(x * 0.04, _seed ^ 0x12345678);
                var largeCave = SimpleNoise(x * 0.015, _seed ^ 0x9ABCDEF0);
                if (caveValue > 0.35 || largeCave > 0.45)
                {
                    _tileMap.SetTile(x, y, Tile.Air);
                }
            }
        }
    }

    private void GenerateOres()
    {
        for (var x = 0; x < _worldWidth; x++)
        {
            for (var y = 0; y < _worldHeight; y++)
            {
                if (_tileMap.GetTile(x, y) != Tile.Stone)
                {
                    continue;
                }
                var depth = (float)y / _worldHeight;

                var copperVal = SimpleNoise((x + 1000) * 0.08, _seed ^ 0xFEDCBA98);
                if (copperVal > 0.55 && depth > 0.2)
                {
                    _tileMap.SetTile(x, y, Tile.CopperOre);
                    continue;
                }
                var ironVal = SimpleNoise((x + 2000) * 0.08, _seed ^ 0xFEDCBA98);
                if (ironVal > 0.6 && depth > 0.35)
                {
                    _tileMap.SetTile(x, y, Tile.IronOre);
                    continue;
                }
                var goldVal = SimpleNoise((x + 3000) * 0.08, _seed ^ 0xFEDCBA98);
                if (goldVal > 0.65 && depth > 0.5)
                {
                    _tileMap.SetTile(x, y, Tile.GoldOre);
                }
            }
        }
    }

    private void GenerateTrees()
    {
        var rng = new Random((int)_seed);
        for (var x = 2; x < _worldWidth - 2; x++)
        {
            if (rng.NextDouble() > 0.08)
            {
                continue;
            }
            var biome = GetBiomeAt(x);
            if (biome == Biome.Desert || biome == Biome.Ocean)
            {
                continue;
            }
            for (var y = 0; y < _worldHeight - 1; y++)
            {
                if (_tileMap.GetTile(x, y) != Tile.Air)
                {
                    continue;
                }
                var below = _tileMap.GetTile(x, y + 1);
                if (below != Tile.Grass && below != Tile.Sand)
                {
                    continue;
                }
                var treeHeight = rng.Next(4, 8);
                for (var ty = 0; ty < treeHeight; ty++)
                {
                    if (y - ty >= 0)
                    {
                        _tileMap.SetTile(x, y - ty, Tile.Wood);
                    }
                }
                var leafStart = y - treeHeight;
                for (var lx = -2; lx <= 2; lx++)
                {
                    for (var ly = -2; ly <= 0; ly++)
                    {
                        var tx = x + lx;
                        var ty = leafStart + ly;
                        if (tx >= 0 && tx < _worldWidth && ty >= 0 && ty < _worldHeight)
                        {
                            if (_tileMap.GetTile(tx, ty) == Tile.Air)
                            {
                                _tileMap.SetTile(tx, ty, Tile.Leaf);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    private int FindSpawnPoint()
    {
        var centerX = _worldWidth / 2;
        for (var dx = 0; dx < _worldWidth / 2; dx++)
        {
            for (var dir = -1; dir <= 1; dir += 2)
            {
                var x = centerX + dx * dir;
                if (x < 0 || x >= _worldWidth)
                {
                    continue;
                }
                for (var y = 0; y < _worldHeight - 1; y++)
                {
                    if (_tileMap.GetTile(x, y) == Tile.Air && _tileMap.GetTile(x, y + 1) != Tile.Air)
                    {
                        return y;
                    }
                }
            }
        }
        return _worldHeight / 3;
    }

    private void CreatePlayer(int spawnX, int spawnY)
    {
        var player = _world.CreateEntity();
        _world.AddComponent(player, new PlayerTag { PlayerId = 0 });
        _world.AddComponent(player, new TilePosition { X = spawnX, Y = spawnY });
        _world.AddComponent(player, new Velocity { Vx = 0.0f, Vy = 0.0f });
        _world.AddComponent(player, new Health { Current = 100, Max = 100 });
        _world.AddComponent(player, new Inventory { Slots = 10, SelectedSlot = 0 });
        _world.AddComponent(player, new CameraFollow { OffsetX = 0.0f, OffsetY = -5.0f, Smoothing = 0.1f });
    }

    private void CreateEnemies()
    {
        var rng = new Random((int)_seed + 1);
        var enemyCount = 5;
        for (var i = 0; i < enemyCount; i++)
        {
            var enemyType = rng.Next(0, 3);
            var x = rng.Next(10, _worldWidth - 10);
            var surfaceY = FindSurfaceAt(x);
            if (surfaceY < 0)
            {
                continue;
            }
            var enemy = _world.CreateEntity();
            _world.AddComponent(enemy, new EnemyTag { EnemyType = enemyType, AiState = AiState.Idle });
            _world.AddComponent(enemy, new TilePosition { X = x, Y = surfaceY - 1 });
            _world.AddComponent(enemy, new Velocity { Vx = 0.0f, Vy = 0.0f });
            _world.AddComponent(enemy, new Health { Current = GetEnemyMaxHealth(enemyType), Max = GetEnemyMaxHealth(enemyType) });
        }
    }

    private int FindSurfaceAt(int x)
    {
        for (var y = 0; y < _worldHeight - 1; y++)
        {
            if (_tileMap.GetTile(x, y) == Tile.Air && _tileMap.GetTile(x, y + 1) != Tile.Air)
            {
                return y + 1;
            }
        }
        return -1;
    }

    private int GetBiomeAt(int x)
    {
        var t = (float)x / _worldWidth;
        if (t < 0.1f || t > 0.9f) return Biome.Ocean;
        if (t < 0.2f) return Biome.Desert;
        if (t < 0.35f) return Biome.Forest;
        if (t < 0.45f) return Biome.Corruption;
        if (t < 0.55f) return Biome.Jungle;
        if (t < 0.65f) return Biome.Hallow;
        if (t < 0.8f) return Biome.Tundra;
        return Biome.Forest;
    }

    private static int GetSurfaceTile(int biome)
    {
        return biome switch
        {
            Biome.Desert => Tile.Sand,
            Biome.Tundra => Tile.Snow,
            _ => Tile.Grass
        };
    }

    private static int GetSubsurfaceTile(int biome)
    {
        return biome switch
        {
            Biome.Desert => Tile.Sand,
            Biome.Tundra => Tile.Ice,
            _ => Tile.Dirt
        };
    }

    private static int GetEnemyMaxHealth(int enemyType)
    {
        return enemyType switch
        {
            EnemyType.Slime => 30,
            EnemyType.Zombie => 50,
            EnemyType.DemonEye => 40,
            _ => 30
        };
    }

    private static float SimpleNoise(float x, ulong seed)
    {
        var n = (long)(x * 127.1 + seed * 311.7);
        n = (n << 13) ^ n;
        return (1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f) * 0.5f + 0.5f;
    }
}

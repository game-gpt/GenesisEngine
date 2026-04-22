using Genesis.Terraria.Components;

namespace Genesis.Terraria.World;

public sealed class TerrariaWorldGenerator
{
    private readonly SimplexNoise _surfaceNoise;
    private readonly SimplexNoise _caveNoise;
    private readonly SimplexNoise _oreNoise;
    private readonly SimplexNoise _biomeNoise;
    private readonly ulong _seed;

    public TerrariaWorldGenerator(ulong seed)
    {
        _seed = seed;
        _surfaceNoise = new SimplexNoise(seed);
        _caveNoise = new SimplexNoise(seed ^ 0x123456789ABCDEF);
        _oreNoise = new SimplexNoise(seed ^ 0xFEDCBA987654321);
        _biomeNoise = new SimplexNoise(seed ^ 0xAAAAAAAAAAAAAAAA);
    }

    public void Generate(TileMapData tileMap)
    {
        GenerateTerrain(tileMap);
        GenerateCaves(tileMap);
        GenerateOres(tileMap);
        GenerateTrees(tileMap);
    }

    private void GenerateTerrain(TileMapData tileMap)
    {
        var surfaceBase = tileMap.Height * 0.3;

        for (var x = 0; x < tileMap.Width; x++)
        {
            var surfaceHeight = CalculateSurfaceHeight(x, surfaceBase, tileMap.Height);
            var biome = GetBiomeAt(x, tileMap.Width);

            for (var y = 0; y < tileMap.Height; y++)
            {
                if (y < surfaceHeight)
                {
                    tileMap.SetTile(x, y, TileId.Air);
                }
                else if (y == (int)surfaceHeight)
                {
                    tileMap.SetTile(x, y, GetSurfaceTile(biome));
                }
                else if (y < surfaceHeight + 5)
                {
                    tileMap.SetTile(x, y, GetSubsurfaceTile(biome));
                }
                else
                {
                    tileMap.SetTile(x, y, TileId.Stone);
                }
            }
        }
    }

    private double CalculateSurfaceHeight(int x, double baseHeight, int worldHeight)
    {
        var macro = _surfaceNoise.FractalNoise2D(x * 0.002, 0, 6, 0.5, 2.0);
        var hills = _surfaceNoise.FractalNoise2D(x * 0.01, 100, 4, 0.5, 2.0);
        var detail = _surfaceNoise.FractalNoise2D(x * 0.05, 200, 2, 0.5, 2.0);

        var height = baseHeight + macro * worldHeight * 0.15 + hills * 15 + detail * 3;

        return Math.Clamp(height, 5, worldHeight - 20);
    }

    private void GenerateCaves(TileMapData tileMap)
    {
        for (var x = 0; x < tileMap.Width; x++)
        {
            for (var y = 0; y < tileMap.Height; y++)
            {
                if (tileMap.GetTile(x, y) == TileId.Air)
                {
                    continue;
                }

                var caveValue = _caveNoise.FractalNoise2D(x * 0.04, y * 0.04, 3, 0.5, 2.0);
                var largeCave = _caveNoise.FractalNoise2D(x * 0.015, y * 0.015, 2, 0.5, 2.0);

                if (caveValue > 0.35 || largeCave > 0.45)
                {
                    tileMap.SetTile(x, y, TileId.Air);
                }
            }
        }
    }

    private void GenerateOres(TileMapData tileMap)
    {
        for (var x = 0; x < tileMap.Width; x++)
        {
            for (var y = 0; y < tileMap.Height; y++)
            {
                if (tileMap.GetTile(x, y) != TileId.Stone)
                {
                    continue;
                }

                var depth = y / (float)tileMap.Height;

                var copperVal = _oreNoise.FractalNoise2D(x * 0.08 + 1000, y * 0.08, 2, 0.5, 2.0);
                if (copperVal > 0.55 && depth > 0.2)
                {
                    tileMap.SetTile(x, y, TileId.CopperOre);
                    continue;
                }

                var ironVal = _oreNoise.FractalNoise2D(x * 0.08 + 2000, y * 0.08, 2, 0.5, 2.0);
                if (ironVal > 0.6 && depth > 0.35)
                {
                    tileMap.SetTile(x, y, TileId.IronOre);
                    continue;
                }

                var goldVal = _oreNoise.FractalNoise2D(x * 0.08 + 3000, y * 0.08, 2, 0.5, 2.0);
                if (goldVal > 0.65 && depth > 0.5)
                {
                    tileMap.SetTile(x, y, TileId.GoldOre);
                }
            }
        }
    }

    private void GenerateTrees(TileMapData tileMap)
    {
        var rng = new Random((int)(_seed ^ 0xDEADBEEF));

        for (var x = 2; x < tileMap.Width - 2; x++)
        {
            if (rng.NextDouble() > 0.08)
            {
                continue;
            }

            var biome = GetBiomeAt(x, tileMap.Width);
            if (biome is BiomeId.Desert or BiomeId.Ocean)
            {
                continue;
            }

            for (var y = 0; y < tileMap.Height - 1; y++)
            {
                if (tileMap.GetTile(x, y) != TileId.Air)
                {
                    continue;
                }

                if (tileMap.GetTile(x, y + 1) != TileId.Grass && tileMap.GetTile(x, y + 1) != TileId.Sand)
                {
                    continue;
                }

                var treeHeight = rng.Next(4, 8);
                for (var ty = 0; ty < treeHeight && y - ty >= 0; ty++)
                {
                    tileMap.SetTile(x, y - ty, TileId.Wood);
                }

                var leafStart = y - treeHeight;
                for (var lx = -2; lx <= 2; lx++)
                {
                    for (var ly = -2; ly <= 0; ly++)
                    {
                        var tx = x + lx;
                        var ty = leafStart + ly;
                        if (tx >= 0 && tx < tileMap.Width && ty >= 0 && ty < tileMap.Height)
                        {
                            if (tileMap.GetTile(tx, ty) == TileId.Air)
                            {
                                tileMap.SetTile(tx, ty, TileId.Leaf);
                            }
                        }
                    }
                }

                break;
            }
        }
    }

    private int GetBiomeAt(int x, int worldWidth)
    {
        var biomeValue = _biomeNoise.FractalNoise2D(x * 0.001, 0, 3, 0.5, 2.0);
        var normalized = (biomeValue + 1.0) * 0.5;

        if (x < worldWidth * 0.1 || x > worldWidth * 0.9)
        {
            return BiomeId.Ocean;
        }

        if (x < worldWidth * 0.15 || x > worldWidth * 0.85)
        {
            return BiomeId.Desert;
        }

        return normalized switch
        {
            < 0.2 => BiomeId.Tundra,
            < 0.35 => BiomeId.Forest,
            < 0.5 => BiomeId.Jungle,
            < 0.65 => BiomeId.Corruption,
            < 0.8 => BiomeId.Hallow,
            _ => BiomeId.Forest
        };
    }

    private static int GetSurfaceTile(int biome)
    {
        return biome switch
        {
            BiomeId.Desert => TileId.Sand,
            BiomeId.Tundra => TileId.Snow,
            BiomeId.Ocean => TileId.Sand,
            _ => TileId.Grass
        };
    }

    private static int GetSubsurfaceTile(int biome)
    {
        return biome switch
        {
            BiomeId.Desert => TileId.Sand,
            BiomeId.Tundra => TileId.Ice,
            _ => TileId.Dirt
        };
    }

    public int FindSpawnPoint(TileMapData tileMap)
    {
        var centerX = tileMap.Width / 2;

        for (var dx = 0; dx < tileMap.Width / 2; dx++)
        {
            for (var dir = -1; dir <= 1; dir += 2)
            {
                var x = centerX + dx * dir;
                if (x < 0 || x >= tileMap.Width)
                {
                    continue;
                }

                for (var y = 0; y < tileMap.Height - 1; y++)
                {
                    if (tileMap.GetTile(x, y) == TileId.Air && tileMap.GetTile(x, y + 1) != TileId.Air)
                    {
                        return y;
                    }
                }
            }
        }

        return tileMap.Height / 3;
    }
}

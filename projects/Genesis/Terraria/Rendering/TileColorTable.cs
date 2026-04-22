using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;
using Gnosis.Rendering.Backends.Software;
using Gnosis.Rendering.Shader;
using Gnosis.Rendering.TwoD;

namespace Genesis.Terraria.Rendering;

public sealed class TileColorTable
{
    private readonly Dictionary<int, (float r, float g, float b)> _colors = new()
    {
        [TileId.Air] = (0.53f, 0.81f, 0.92f),
        [TileId.Dirt] = (0.55f, 0.35f, 0.17f),
        [TileId.Stone] = (0.50f, 0.50f, 0.50f),
        [TileId.Grass] = (0.13f, 0.55f, 0.13f),
        [TileId.Sand] = (0.82f, 0.71f, 0.55f),
        [TileId.Water] = (0.12f, 0.56f, 1.0f),
        [TileId.Wood] = (0.55f, 0.27f, 0.07f),
        [TileId.Leaf] = (0.0f, 0.39f, 0.0f),
        [TileId.IronOre] = (0.63f, 0.32f, 0.18f),
        [TileId.GoldOre] = (1.0f, 0.84f, 0.0f),
        [TileId.CopperOre] = (0.72f, 0.45f, 0.20f),
        [TileId.Snow] = (0.94f, 0.94f, 1.0f),
        [TileId.Ice] = (0.68f, 0.85f, 0.90f),
        [TileId.Clay] = (0.63f, 0.32f, 0.18f),
        [TileId.Boundary] = (0.15f, 0.15f, 0.15f)
    };

    private readonly Dictionary<int, (float r, float g, float b)> _enemyColors = new()
    {
        [EnemyType.Slime] = (0.0f, 0.78f, 0.0f),
        [EnemyType.Zombie] = (0.0f, 0.59f, 0.0f),
        [EnemyType.DemonEye] = (0.78f, 0.0f, 0.0f)
    };

    public (float r, float g, float b) GetTileColor(int tileId)
    {
        return _colors.TryGetValue(tileId, out var c) ? c : (1.0f, 0.0f, 1.0f);
    }

    public (float r, float g, float b) GetEnemyColor(int enemyType)
    {
        return _enemyColors.TryGetValue(enemyType, out var c) ? c : (1.0f, 0.0f, 0.0f);
    }

    public static (float r, float g, float b) ApplyDaylight((float r, float g, float b) color, float daylight)
    {
        var factor = 0.3f + 0.7f * daylight;
        return (color.r * factor, color.g * factor, color.b * factor);
    }
}

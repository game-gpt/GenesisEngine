namespace Genesis.Game.Terraria.Rendering;

public sealed class TileColorTable
{
    private readonly (float R, float G, float B)[] _tileColors;
    private readonly (float R, float G, float B)[] _enemyColors;

    public float AmbientMin { get; } = 0.3f;
    public float AmbientRange { get; } = 0.7f;
    public (float R, float G, float B) SkyColor { get; } = (0.53f, 0.81f, 0.92f);
    public (float R, float G, float B) UndergroundColor { get; } = (0.31f, 0.24f, 0.16f);
    public float UndergroundDepthRatio { get; } = 0.3f;
    public float UndergroundLightFactor { get; } = 0.3f;

    public TileColorTable()
    {
        _tileColors = new (float, float, float)[100];
        _tileColors[0] = (0.53f, 0.81f, 0.92f);
        _tileColors[1] = (0.55f, 0.35f, 0.17f);
        _tileColors[2] = (0.50f, 0.50f, 0.50f);
        _tileColors[3] = (0.13f, 0.55f, 0.13f);
        _tileColors[4] = (0.82f, 0.71f, 0.55f);
        _tileColors[5] = (0.12f, 0.56f, 1.0f);
        _tileColors[6] = (0.55f, 0.27f, 0.07f);
        _tileColors[7] = (0.0f, 0.39f, 0.0f);
        _tileColors[8] = (0.63f, 0.32f, 0.18f);
        _tileColors[9] = (1.0f, 0.84f, 0.0f);
        _tileColors[10] = (0.72f, 0.45f, 0.20f);
        _tileColors[11] = (0.94f, 0.94f, 1.0f);
        _tileColors[12] = (0.68f, 0.85f, 0.90f);
        _tileColors[13] = (0.63f, 0.32f, 0.18f);
        _tileColors[99] = (0.15f, 0.15f, 0.15f);

        _enemyColors = new (float, float, float)[3];
        _enemyColors[0] = (0.0f, 0.78f, 0.0f);
        _enemyColors[1] = (0.0f, 0.59f, 0.0f);
        _enemyColors[2] = (0.78f, 0.0f, 0.0f);
    }

    public (float R, float G, float B) GetTileColor(int tileId)
    {
        if (tileId >= 0 && tileId < _tileColors.Length && _tileColors[tileId] != default)
        {
            return _tileColors[tileId];
        }
        return (0.5f, 0.0f, 0.5f);
    }

    public (float R, float G, float B) GetEnemyColor(int enemyType)
    {
        if (enemyType >= 0 && enemyType < _enemyColors.Length)
        {
            return _enemyColors[enemyType];
        }
        return (1.0f, 0.0f, 1.0f);
    }
}

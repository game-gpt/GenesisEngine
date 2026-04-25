namespace Genesis.Game.Terraria.Game;

public sealed class TileMap
{
    private readonly int[,] _tiles;

    public int Width { get; }
    public int Height { get; }

    public TileMap(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new int[width, height];
    }

    public int GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return Tile.Boundary;
        }
        return _tiles[x, y];
    }

    public void SetTile(int x, int y, int tileId)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }
        _tiles[x, y] = tileId;
    }

    public bool IsSolid(int x, int y)
    {
        return Tile.IsSolid(GetTile(x, y));
    }
}

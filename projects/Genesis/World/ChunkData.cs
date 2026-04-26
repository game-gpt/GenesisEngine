using Genesis.Core;

namespace Genesis.World;

public sealed class ChunkData
{
    #region 字段

    private readonly int[] _tiles;
    private readonly Dictionary<string, object> _metadata;

    #endregion

    #region 属性

    public int ChunkX { get; }
    public int ChunkZ { get; }
    public int Width { get; }
    public int Height { get; }
    public BiomeType Biome { get; set; }
    public ulong RegionHash { get; set; }
    public bool IsGenerated { get; set; }
    public bool IsDirty { get; set; }
    public Timestamp LastModified { get; set; }
    public int EntityCount { get; set; }

    #endregion

    #region 构造函数

    public ChunkData(int chunkX, int chunkZ, int width, int height)
    {
        ChunkX = chunkX;
        ChunkZ = chunkZ;
        Width = width;
        Height = height;
        Biome = BiomeType.Plains;
        RegionHash = 0;
        IsGenerated = false;
        IsDirty = false;
        LastModified = new Timestamp(DateTime.MinValue);
        EntityCount = 0;

        _tiles = new int[width * height];
        _metadata = new Dictionary<string, object>();
    }

    #endregion

    #region 瓦片访问

    public int GetTile(int x, int z)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Height)
        {
            return -1;
        }

        return _tiles[z * Width + x];
    }

    public void SetTile(int x, int z, int tileType)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Height)
        {
            return;
        }

        _tiles[z * Width + x] = tileType;
        IsDirty = true;
    }

    public int[] GetAllTiles()
    {
        return _tiles;
    }

    public void SetAllTiles(int[] tiles)
    {
        if (tiles.Length != _tiles.Length)
        {
            return;
        }

        Array.Copy(tiles, _tiles, _tiles.Length);
        IsDirty = true;
    }

    #endregion

    #region 元数据

    public void SetMetadata(string key, object value)
    {
        _metadata[key] = value;
    }

    public T? GetMetadata<T>(string key)
    {
        if (_metadata.TryGetValue(key, out var value) && value is T typed)
        {
            return typed;
        }

        return default;
    }

    public bool HasMetadata(string key)
    {
        return _metadata.ContainsKey(key);
    }

    #endregion

    #region 坐标转换

    public int WorldToLocalX(int worldX)
    {
        return worldX - ChunkX * Width;
    }

    public int WorldToLocalZ(int worldZ)
    {
        return worldZ - ChunkZ * Height;
    }

    public int LocalToWorldX(int localX)
    {
        return ChunkX * Width + localX;
    }

    public int LocalToWorldZ(int localZ)
    {
        return ChunkZ * Height + localZ;
    }

    public Bounds GetWorldBounds()
    {
        var minX = ChunkX * Width;
        var minZ = ChunkZ * Height;
        return new Bounds(minX, 0, minZ, minX + Width, 0, minZ + Height);
    }

    #endregion

    #region 邻居查询

    public (int chunkX, int chunkZ) GetNeighborChunk(Direction direction)
    {
        return direction switch
        {
            Direction.North => (ChunkX, ChunkZ - 1),
            Direction.South => (ChunkX, ChunkZ + 1),
            Direction.East => (ChunkX + 1, ChunkZ),
            Direction.West => (ChunkX - 1, ChunkZ),
            _ => (ChunkX, ChunkZ)
        };
    }

    #endregion
}

public enum Direction
{
    North,
    South,
    East,
    West
}

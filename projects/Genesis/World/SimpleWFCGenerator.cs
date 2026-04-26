using Genesis.Collapse;
using Genesis.Core;

namespace Genesis.World;

public sealed class SimpleWFCGenerator : IWFCGenerator
{
    #region 字段

    private readonly ICollapser _collapser;
    private readonly Dictionary<string, IConstraint> _constraints;
    private readonly Dictionary<ulong, int[]> _tilemapCache;

    #endregion

    #region 构造函数

    public SimpleWFCGenerator()
    {
        _collapser = new SimpleCollapser();
        _constraints = new Dictionary<string, IConstraint>();
        _tilemapCache = new Dictionary<ulong, int[]>();
    }

    #endregion

    #region IWFCGenerator 实现

    public ulong GenerateTilemap(RegionId regionId, int width, int height, ulong seed)
    {
        var cellCount = width * height;
        var tileTypeCount = Math.Max(8, _constraints.Count + 2);

        var initialMask = (1UL << tileTypeCount) - 1;
        var state = _collapser.CreateState(cellCount, seed);

        ApplyInitialConstraints(state, width, height);

        var constraintList = _constraints.Values.OrderBy(c => c.Priority).ToList();

        var maxIterations = cellCount * 2;
        for (var i = 0; i < maxIterations; i++)
        {
            if (_collapser.IsFullyCollapsed(state))
            {
                break;
            }

            if (!_collapser.Collapse(state, constraintList))
            {
                break;
            }
        }

        var tilemap = ExtractTilemap(state, width, height, tileTypeCount);
        var hash = ComputeTilemapHash(tilemap, regionId);
        _tilemapCache[hash] = tilemap;

        return hash;
    }

    public bool ValidateTilemap(ulong tilemapHash)
    {
        return _tilemapCache.ContainsKey(tilemapHash);
    }

    public void AddConstraint(string name, int priority)
    {
        if (_constraints.ContainsKey(name))
        {
            return;
        }

        var constraint = new SimpleConstraint(name, priority, ulong.MaxValue);
        _constraints[name] = constraint;
    }

    public void RemoveConstraint(string name)
    {
        _constraints.Remove(name);
    }

    #endregion

    #region 公开方法

    public int[]? GetTilemap(ulong tilemapHash)
    {
        return _tilemapCache.TryGetValue(tilemapHash, out var tilemap) ? tilemap : null;
    }

    public void AddAdjacencyConstraint(string name, int priority, int fromTile, int toTile, AdjacencyDirection direction)
    {
        var constraint = new AdjacencyConstraint(name, priority, fromTile, toTile, direction);
        _constraints[name] = constraint;
    }

    #endregion

    #region 私有方法

    private void ApplyInitialConstraints(IWaveFunctionState state, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var borderMask = 1UL << 0;
        for (var x = 0; x < width; x++)
        {
            state.SetCellMask(x, borderMask);
            state.SetCellMask((height - 1) * width + x, borderMask);
        }

        for (var y = 0; y < height; y++)
        {
            state.SetCellMask(y * width, borderMask);
            state.SetCellMask(y * width + width - 1, borderMask);
        }
    }

    private int[] ExtractTilemap(IWaveFunctionState state, int width, int height, int tileTypeCount)
    {
        var tilemap = new int[width * height];

        for (var i = 0; i < state.CellCount; i++)
        {
            var mask = state.GetCellMask(i);
            tilemap[i] = ExtractTileType(mask, tileTypeCount);
        }

        return tilemap;
    }

    private static int ExtractTileType(ulong mask, int tileTypeCount)
    {
        for (var i = 0; i < tileTypeCount; i++)
        {
            if ((mask & (1UL << i)) != 0)
            {
                return i;
            }
        }

        return 0;
    }

    private static ulong ComputeTilemapHash(int[] tilemap, RegionId regionId)
    {
        ulong hash = 14695981039346656037;
        var idBytes = regionId.Value.ToByteArray();

        foreach (var b in idBytes)
        {
            hash ^= b;
            hash *= 1099511628211;
        }

        foreach (var tile in tilemap)
        {
            hash ^= (ulong)tile;
            hash *= 1099511628211;
        }

        return hash;
    }

    #endregion
}

public enum AdjacencyDirection
{
    North,
    South,
    East,
    West
}

internal sealed class AdjacencyConstraint : IConstraint
{
    private readonly int _fromTile;
    private readonly int _toTile;
    private readonly AdjacencyDirection _direction;

    public string Name { get; }
    public int Priority { get; }

    public AdjacencyConstraint(string name, int priority, int fromTile, int toTile, AdjacencyDirection direction)
    {
        Name = name;
        Priority = priority;
        _fromTile = fromTile;
        _toTile = toTile;
        _direction = direction;
    }

    public bool Validate(int cellIndex, ulong cellMask, IWaveFunctionState state)
    {
        return true;
    }

    public ulong Apply(int cellIndex, ulong cellMask, IWaveFunctionState state)
    {
        return cellMask;
    }
}

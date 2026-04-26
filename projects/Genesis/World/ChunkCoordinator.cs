using Genesis.Attention;
using Genesis.Core;
using Genesis.Spacetime;

namespace Genesis.World;

public sealed class ChunkCoordinator
{
    #region 字段

    private readonly IWorldGenerator _worldGenerator;
    private readonly INoiseGenerator _noiseGenerator;
    private readonly ISpacetimeTree _spacetimeTree;
    private readonly IAttentionManager? _attentionManager;
    private readonly Dictionary<(int chunkX, int chunkZ), ChunkData> _loadedChunks;
    private readonly List<(int chunkX, int chunkZ)> _loadQueue;
    private readonly List<(int chunkX, int chunkZ)> _unloadQueue;
    private readonly int _chunkSize;
    private readonly int _loadRadius;
    private readonly int _unloadRadius;
    private readonly ulong _worldSeed;
    private Position _lastPlayerPosition;
    private bool _needsUpdate;

    #endregion

    #region 属性

    public int LoadedChunkCount => _loadedChunks.Count;
    public int PendingLoadCount => _loadQueue.Count;
    public int PendingUnloadCount => _unloadQueue.Count;
    public IReadOnlyDictionary<(int chunkX, int chunkZ), ChunkData> LoadedChunks => _loadedChunks;

    #endregion

    #region 构造函数

    public ChunkCoordinator(
        IWorldGenerator worldGenerator,
        INoiseGenerator noiseGenerator,
        ISpacetimeTree spacetimeTree,
        ulong worldSeed,
        int chunkSize = 32,
        int loadRadius = 4,
        int unloadRadius = 6,
        IAttentionManager? attentionManager = null)
    {
        _worldGenerator = worldGenerator;
        _noiseGenerator = noiseGenerator;
        _spacetimeTree = spacetimeTree;
        _worldSeed = worldSeed;
        _chunkSize = chunkSize;
        _loadRadius = loadRadius;
        _unloadRadius = unloadRadius;
        _attentionManager = attentionManager;
        _loadedChunks = new Dictionary<(int chunkX, int chunkZ), ChunkData>();
        _loadQueue = new List<(int chunkX, int chunkZ)>();
        _unloadQueue = new List<(int chunkX, int chunkZ)>();
        _lastPlayerPosition = Position.Zero;
        _needsUpdate = true;
    }

    #endregion

    #region 公开方法

    public void Update(Position playerPosition)
    {
        var moved = playerPosition.DistanceToSquared(_lastPlayerPosition) > (_chunkSize * 0.5) * (_chunkSize * 0.5);
        if (moved || _needsUpdate)
        {
            _lastPlayerPosition = playerPosition;
            _needsUpdate = false;
            RecalculateChunkLists(playerPosition);
        }

        ProcessLoadQueue(2);
        ProcessUnloadQueue(2);
    }

    public ChunkData? GetChunk(int chunkX, int chunkZ)
    {
        return _loadedChunks.TryGetValue((chunkX, chunkZ), out var chunk) ? chunk : null;
    }

    public ChunkData? GetChunkAtWorldPosition(int worldX, int worldZ)
    {
        var chunkX = (int)Math.Floor((double)worldX / _chunkSize);
        var chunkZ = (int)Math.Floor((double)worldZ / _chunkSize);
        return GetChunk(chunkX, chunkZ);
    }

    public int GetTileAtWorldPosition(int worldX, int worldZ)
    {
        var chunk = GetChunkAtWorldPosition(worldX, worldZ);
        if (chunk is null || !chunk.IsGenerated)
        {
            return -1;
        }

        var localX = chunk.WorldToLocalX(worldX);
        var localZ = chunk.WorldToLocalZ(worldZ);
        return chunk.GetTile(localX, localZ);
    }

    public void SetTileAtWorldPosition(int worldX, int worldZ, int tileType)
    {
        var chunk = GetChunkAtWorldPosition(worldX, worldZ);
        if (chunk is null || !chunk.IsGenerated)
        {
            return;
        }

        var localX = chunk.WorldToLocalX(worldX);
        var localZ = chunk.WorldToLocalZ(worldZ);
        chunk.SetTile(localX, localZ, tileType);

        PropagateCausalEffect(chunk, localX, localZ);
    }

    public void ForceLoadChunk(int chunkX, int chunkZ)
    {
        if (!_loadedChunks.ContainsKey((chunkX, chunkZ)))
        {
            LoadChunk(chunkX, chunkZ);
        }
    }

    public void ForceUnloadChunk(int chunkX, int chunkZ)
    {
        UnloadChunk(chunkX, chunkZ);
    }

    public List<ChunkData> GetDirtyChunks()
    {
        var dirty = new List<ChunkData>();
        foreach (var chunk in _loadedChunks.Values)
        {
            if (chunk.IsDirty)
            {
                dirty.Add(chunk);
            }
        }

        return dirty;
    }

    public void MarkChunkClean(int chunkX, int chunkZ)
    {
        if (_loadedChunks.TryGetValue((chunkX, chunkZ), out var chunk))
        {
            chunk.IsDirty = false;
        }
    }

    #endregion

    #region 私有方法 - 区块调度

    private void RecalculateChunkLists(Position playerPosition)
    {
        _loadQueue.Clear();
        _unloadQueue.Clear();

        var playerChunkX = (int)Math.Floor(playerPosition.X / _chunkSize);
        var playerChunkZ = (int)Math.Floor(playerPosition.Z / _chunkSize);

        var requiredChunks = new HashSet<(int, int)>();

        for (var dx = -_loadRadius; dx <= _loadRadius; dx++)
        {
            for (var dz = -_loadRadius; dz <= _loadRadius; dz++)
            {
                if (dx * dx + dz * dz > _loadRadius * _loadRadius)
                {
                    continue;
                }

                var cx = playerChunkX + dx;
                var cz = playerChunkZ + dz;
                requiredChunks.Add((cx, cz));

                if (!_loadedChunks.ContainsKey((cx, cz)))
                {
                    _loadQueue.Add((cx, cz));
                }
            }
        }

        if (_attentionManager is not null)
        {
            var highAttentionNodes = _attentionManager.GetHighAttentionNodes();
            foreach (var node in highAttentionNodes)
            {
                var cx = (int)Math.Floor(node.Bounds.Center.X / _chunkSize);
                var cz = (int)Math.Floor(node.Bounds.Center.Z / _chunkSize);

                if (!requiredChunks.Contains((cx, cz)) && !_loadedChunks.ContainsKey((cx, cz)))
                {
                    requiredChunks.Add((cx, cz));
                    _loadQueue.Add((cx, cz));
                }
            }
        }

        _loadQueue.Sort((a, b) =>
        {
            var distA = (a.chunkX - playerChunkX) * (a.chunkX - playerChunkX) +
                        (a.chunkZ - playerChunkZ) * (a.chunkZ - playerChunkZ);
            var distB = (b.chunkX - playerChunkX) * (b.chunkX - playerChunkX) +
                        (b.chunkZ - playerChunkZ) * (b.chunkZ - playerChunkZ);
            return distA.CompareTo(distB);
        });

        foreach (var (cx, cz) in _loadedChunks.Keys)
        {
            if (!requiredChunks.Contains((cx, cz)))
            {
                var distSq = (cx - playerChunkX) * (cx - playerChunkX) +
                             (cz - playerChunkZ) * (cz - playerChunkZ);

                if (distSq > _unloadRadius * _unloadRadius)
                {
                    _unloadQueue.Add((cx, cz));
                }
            }
        }
    }

    #endregion

    #region 私有方法 - 加载/卸载

    private void ProcessLoadQueue(int maxPerFrame)
    {
        var loaded = 0;
        while (_loadQueue.Count > 0 && loaded < maxPerFrame)
        {
            var (cx, cz) = _loadQueue[0];
            _loadQueue.RemoveAt(0);

            if (_loadedChunks.ContainsKey((cx, cz)))
            {
                continue;
            }

            LoadChunk(cx, cz);
            loaded++;
        }
    }

    private void ProcessUnloadQueue(int maxPerFrame)
    {
        var unloaded = 0;
        while (_unloadQueue.Count > 0 && unloaded < maxPerFrame)
        {
            var (cx, cz) = _unloadQueue[0];
            _unloadQueue.RemoveAt(0);
            UnloadChunk(cx, cz);
            unloaded++;
        }
    }

    private void LoadChunk(int chunkX, int chunkZ)
    {
        var chunk = new ChunkData(chunkX, chunkZ, _chunkSize, _chunkSize);

        GenerateChunkTerrain(chunk);

        var regionId = RegionId.New();
        var regionHash = _worldGenerator.GenerateRegion(regionId, _worldSeed);
        chunk.RegionHash = regionHash;
        chunk.IsGenerated = true;

        _loadedChunks[(chunkX, chunkZ)] = chunk;

        RegisterInSpacetimeTree(chunk);
    }

    private void UnloadChunk(int chunkX, int chunkZ)
    {
        if (!_loadedChunks.TryGetValue((chunkX, chunkZ), out var chunk))
        {
            return;
        }

        UnregisterFromSpacetimeTree(chunk);
        _loadedChunks.Remove((chunkX, chunkZ));
    }

    #endregion

    #region 私有方法 - 地形生成

    private void GenerateChunkTerrain(ChunkData chunk)
    {
        var worldGen = (SimpleWorldGenerator)_worldGenerator;

        for (var x = 0; x < chunk.Width; x++)
        {
            for (var z = 0; z < chunk.Height; z++)
            {
                var worldX = chunk.LocalToWorldX(x);
                var worldZ = chunk.LocalToWorldZ(z);

                var elevation = worldGen.GetElevation(worldX, worldZ, _worldSeed);
                var temperature = worldGen.GetTemperature(worldX, worldZ, _worldSeed);
                var moisture = worldGen.GetMoisture(worldX, worldZ, _worldSeed);

                var biome = worldGen.DetermineBiome(temperature, moisture, elevation);
                var tile = BiomeToTile(biome, elevation);

                chunk.SetTile(x, z, tile);
            }
        }

        var centerElevation = worldGen.GetElevation(
            chunk.LocalToWorldX(_chunkSize / 2),
            chunk.LocalToWorldZ(_chunkSize / 2),
            _worldSeed);
        var centerTemp = worldGen.GetTemperature(
            chunk.LocalToWorldX(_chunkSize / 2),
            chunk.LocalToWorldZ(_chunkSize / 2),
            _worldSeed);
        var centerMoisture = worldGen.GetMoisture(
            chunk.LocalToWorldX(_chunkSize / 2),
            chunk.LocalToWorldZ(_chunkSize / 2),
            _worldSeed);

        chunk.Biome = worldGen.DetermineBiome(centerTemp, centerMoisture, centerElevation);
    }

    private static int BiomeToTile(BiomeType biome, double elevation)
    {
        return biome switch
        {
            BiomeType.Ocean => 0,
            BiomeType.Plains => 1,
            BiomeType.Forest => 2,
            BiomeType.Desert => 3,
            BiomeType.Mountains => elevation > 0.9 ? 5 : 4,
            BiomeType.Tundra => 6,
            BiomeType.Jungle => 7,
            BiomeType.Swamp => 8,
            BiomeType.Volcanic => 9,
            BiomeType.Crystal => 10,
            _ => 1
        };
    }

    #endregion

    #region 私有方法 - 时空树集成

    private void RegisterInSpacetimeTree(ChunkData chunk)
    {
        var bounds = chunk.GetWorldBounds();
        var spatialHash = SpatialHasher.ComputeSpatialHash(
            bounds.Center,
            NodeLevel.L0,
            _worldSeed);

        var node = new HChunkNode(spatialHash, NodeLevel.L0, bounds, _worldSeed);
        _spacetimeTree.InsertNode(node);
    }

    private void UnregisterFromSpacetimeTree(ChunkData chunk)
    {
        var bounds = chunk.GetWorldBounds();
        var spatialHash = SpatialHasher.ComputeSpatialHash(
            bounds.Center,
            NodeLevel.L0,
            _worldSeed);

        _spacetimeTree.RemoveNode(spatialHash);
    }

    #endregion

    #region 私有方法 - 因果传播

    private void PropagateCausalEffect(ChunkData chunk, int localX, int localZ)
    {
        _spacetimeTree.UpdateHistoryHash(
            SpatialHasher.ComputeSpatialHash(
                chunk.GetWorldBounds().Center,
                NodeLevel.L0,
                _worldSeed));

        if (localX == 0)
        {
            PropagateToNeighbor(chunk, Direction.West);
        }
        else if (localX == chunk.Width - 1)
        {
            PropagateToNeighbor(chunk, Direction.East);
        }

        if (localZ == 0)
        {
            PropagateToNeighbor(chunk, Direction.North);
        }
        else if (localZ == chunk.Height - 1)
        {
            PropagateToNeighbor(chunk, Direction.South);
        }
    }

    private void PropagateToNeighbor(ChunkData chunk, Direction direction)
    {
        var (nx, nz) = chunk.GetNeighborChunk(direction);
        var neighbor = GetChunk(nx, nz);
        if (neighbor is null || !neighbor.IsGenerated)
        {
            return;
        }

        neighbor.IsDirty = true;

        var neighborHash = SpatialHasher.ComputeSpatialHash(
            neighbor.GetWorldBounds().Center,
            NodeLevel.L0,
            _worldSeed);
        _spacetimeTree.UpdateHistoryHash(neighborHash);
    }

    #endregion
}

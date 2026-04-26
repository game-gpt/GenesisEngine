using Genesis.World;
using Xunit;

namespace Genesis.Tests.World;

public class ChunkDataTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var chunk = new ChunkData(3, 5, 32, 32);
        Assert.Equal(3, chunk.ChunkX);
        Assert.Equal(5, chunk.ChunkZ);
        Assert.Equal(32, chunk.Width);
        Assert.Equal(32, chunk.Height);
        Assert.False(chunk.IsGenerated);
        Assert.False(chunk.IsDirty);
    }

    [Fact]
    public void SetTile_GetTile_ReturnsValue()
    {
        var chunk = new ChunkData(0, 0, 16, 16);
        chunk.SetTile(5, 10, 7);
        Assert.Equal(7, chunk.GetTile(5, 10));
        Assert.True(chunk.IsDirty);
    }

    [Fact]
    public void GetTile_OutOfBounds_ReturnsMinusOne()
    {
        var chunk = new ChunkData(0, 0, 16, 16);
        Assert.Equal(-1, chunk.GetTile(-1, 0));
        Assert.Equal(-1, chunk.GetTile(0, 16));
        Assert.Equal(-1, chunk.GetTile(16, 0));
    }

    [Fact]
    public void WorldToLocal_ConvertsCorrectly()
    {
        var chunk = new ChunkData(2, 3, 32, 32);
        Assert.Equal(5, chunk.WorldToLocalX(69));
        Assert.Equal(10, chunk.WorldToLocalZ(106));
    }

    [Fact]
    public void LocalToWorld_ConvertsCorrectly()
    {
        var chunk = new ChunkData(2, 3, 32, 32);
        Assert.Equal(69, chunk.LocalToWorldX(5));
        Assert.Equal(106, chunk.LocalToWorldZ(10));
    }

    [Fact]
    public void GetNeighborChunk_ReturnsCorrectCoordinates()
    {
        var chunk = new ChunkData(2, 3, 32, 32);
        var (nx, nz) = chunk.GetNeighborChunk(Direction.North);
        Assert.Equal(2, nx);
        Assert.Equal(2, nz);

        var (ex, ez) = chunk.GetNeighborChunk(Direction.East);
        Assert.Equal(3, ex);
        Assert.Equal(3, ez);
    }

    [Fact]
    public void Metadata_SetGet_Works()
    {
        var chunk = new ChunkData(0, 0, 16, 16);
        chunk.SetMetadata("test_key", 42);
        Assert.Equal(42, chunk.GetMetadata<int>("test_key"));
        Assert.True(chunk.HasMetadata("test_key"));
    }

    [Fact]
    public void SetAllTiles_CopiesArray()
    {
        var chunk = new ChunkData(0, 0, 4, 4);
        var tiles = new int[16];
        for (var i = 0; i < 16; i++) tiles[i] = i;
        chunk.SetAllTiles(tiles);
        Assert.Equal(5, chunk.GetTile(1, 1));
    }
}

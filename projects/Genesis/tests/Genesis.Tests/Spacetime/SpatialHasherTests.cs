using Genesis.Core;
using Genesis.Spacetime;
using Xunit;

namespace Genesis.Tests.Spacetime;

public class SpatialHasherTests
{
    [Fact]
    public void ComputeSpatialHash_IsDeterministic()
    {
        var position = new Position(10.0, 20.0, 30.0);
        var level = NodeLevel.L1;
        var seed = 42UL;

        var hash1 = SpatialHasher.ComputeSpatialHash(position, level, seed);
        var hash2 = SpatialHasher.ComputeSpatialHash(position, level, seed);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeSpatialHash_DifferentPositions_DifferentHashes()
    {
        var seed = 42UL;
        var level = NodeLevel.L0;
        var p1 = new Position(0, 0, 0);
        var p2 = new Position(1, 0, 0);

        var hash1 = SpatialHasher.ComputeSpatialHash(p1, level, seed);
        var hash2 = SpatialHasher.ComputeSpatialHash(p2, level, seed);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeSpatialHash_DifferentLevels_DifferentHashes()
    {
        var position = new Position(10, 10, 10);
        var seed = 42UL;

        var hash0 = SpatialHasher.ComputeSpatialHash(position, NodeLevel.L0, seed);
        var hash1 = SpatialHasher.ComputeSpatialHash(position, NodeLevel.L1, seed);

        Assert.NotEqual(hash0, hash1);
    }

    [Fact]
    public void ComputeSpatialHash_DifferentSeeds_DifferentHashes()
    {
        var position = new Position(10, 10, 10);
        var level = NodeLevel.L0;

        var hash1 = SpatialHasher.ComputeSpatialHash(position, level, 1);
        var hash2 = SpatialHasher.ComputeSpatialHash(position, level, 2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void CombineHash_IsDeterministic()
    {
        var result1 = SpatialHasher.CombineHash(100, 200);
        var result2 = SpatialHasher.CombineHash(100, 200);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void CombineHash_DifferentInputs_DifferentResults()
    {
        var result1 = SpatialHasher.CombineHash(100, 200);
        var result2 = SpatialHasher.CombineHash(100, 300);

        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void ComputeFromChildren_EmptyList_ReturnsZero()
    {
        var result = SpatialHasher.ComputeFromChildren([]);

        Assert.Equal(0UL, result);
    }

    [Fact]
    public void ComputeFromChildren_SingleChild_ReturnsChildHistoryHash()
    {
        var child = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));

        var result = SpatialHasher.ComputeFromChildren([child]);

        Assert.Equal(child.HistoryHash, result);
    }

    [Fact]
    public void ComputeFromChildren_MultipleChildren_CombinesHashes()
    {
        var child1 = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));
        var child2 = new HChunkNode(2, NodeLevel.L0, new Bounds(1, 0, 0, 2, 1, 1));

        var result = SpatialHasher.ComputeFromChildren([child1, child2]);

        var expected = SpatialHasher.CombineHash(child1.HistoryHash, child2.HistoryHash);
        Assert.Equal(expected, result);
    }
}

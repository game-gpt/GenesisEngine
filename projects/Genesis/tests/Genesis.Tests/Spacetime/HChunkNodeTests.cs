using Genesis.Core;
using Genesis.Spacetime;
using Xunit;

namespace Genesis.Tests.Spacetime;

public class HChunkNodeTests
{
    #region 构造函数测试

    [Fact]
    public void Constructor_WithWorldSeed_RecalculatesSpatialHash()
    {
        var bounds = new Bounds(0, 0, 0, 100, 100, 100);
        var worldSeed = 42UL;

        var node = new HChunkNode(0, NodeLevel.L1, bounds, worldSeed);

        var expectedHash = SpatialHasher.ComputeSpatialHash(bounds.Center, NodeLevel.L1, worldSeed);
        Assert.Equal(expectedHash, node.SpatialHash);
    }

    [Fact]
    public void Constructor_WithoutWorldSeed_UsesProvidedHash()
    {
        var hash = 12345UL;
        var bounds = new Bounds(0, 0, 0, 100, 100, 100);

        var node = new HChunkNode(hash, NodeLevel.L1, bounds);

        Assert.Equal(hash, node.SpatialHash);
    }

    [Fact]
    public void Constructor_InitializesAsSuperposition()
    {
        var node = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));

        Assert.Equal(CollapseState.Superposition, node.CollapseState);
    }

    [Fact]
    public void Constructor_InitialTimeScaleByLevel()
    {
        var l0 = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));
        var l1 = new HChunkNode(2, NodeLevel.L1, new Bounds(0, 0, 0, 1, 1, 1));
        var l2 = new HChunkNode(3, NodeLevel.L2, new Bounds(0, 0, 0, 1, 1, 1));
        var l3 = new HChunkNode(4, NodeLevel.L3, new Bounds(0, 0, 0, 1, 1, 1));

        Assert.Equal(1.0, l0.TimeScale);
        Assert.Equal(60.0, l1.TimeScale);
        Assert.Equal(3600.0, l2.TimeScale);
        Assert.Equal(86400.0, l3.TimeScale);
    }

    [Fact]
    public void Constructor_StartsNotDirty()
    {
        var node = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));

        Assert.False(node.IsDirty);
    }

    [Fact]
    public void Constructor_StartsWithNoParent()
    {
        var node = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));

        Assert.Null(node.Parent);
    }

    [Fact]
    public void Constructor_StartsWithNoChildren()
    {
        var node = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));

        Assert.Empty(node.Children);
    }

    #endregion

    #region AddChild / RemoveChild 测试

    [Fact]
    public void AddChild_AddsToChildrenList()
    {
        var parent = new HChunkNode(1, NodeLevel.L1, new Bounds(0, 0, 0, 100, 100, 100));
        var child = new HChunkNode(2, NodeLevel.L0, new Bounds(0, 0, 0, 10, 10, 10));

        parent.AddChild(child);

        Assert.Single(parent.Children);
        Assert.Equal(child.SpatialHash, parent.Children[0].SpatialHash);
    }

    [Fact]
    public void AddChild_SetsParentReference()
    {
        var parent = new HChunkNode(1, NodeLevel.L1, new Bounds(0, 0, 0, 100, 100, 100));
        var child = new HChunkNode(2, NodeLevel.L0, new Bounds(0, 0, 0, 10, 10, 10));

        parent.AddChild(child);

        Assert.Same(parent, child.Parent);
    }

    [Fact]
    public void AddChild_MarksParentDirty()
    {
        var parent = new HChunkNode(1, NodeLevel.L1, new Bounds(0, 0, 0, 100, 100, 100));
        var child = new HChunkNode(2, NodeLevel.L0, new Bounds(0, 0, 0, 10, 10, 10));

        parent.AddChild(child);

        Assert.True(parent.IsDirty);
    }

    [Fact]
    public void RemoveChild_RemovesFromChildrenList()
    {
        var parent = new HChunkNode(1, NodeLevel.L1, new Bounds(0, 0, 0, 100, 100, 100));
        var child = new HChunkNode(2, NodeLevel.L0, new Bounds(0, 0, 0, 10, 10, 10));
        parent.AddChild(child);

        parent.RemoveChild(child);

        Assert.Empty(parent.Children);
    }

    [Fact]
    public void RemoveChild_ClearsParentReference()
    {
        var parent = new HChunkNode(1, NodeLevel.L1, new Bounds(0, 0, 0, 100, 100, 100));
        var child = new HChunkNode(2, NodeLevel.L0, new Bounds(0, 0, 0, 10, 10, 10));
        parent.AddChild(child);

        parent.RemoveChild(child);

        Assert.Null(child.Parent);
    }

    [Fact]
    public void RemoveChild_MarksParentDirty()
    {
        var parent = new HChunkNode(1, NodeLevel.L1, new Bounds(0, 0, 0, 100, 100, 100));
        var child = new HChunkNode(2, NodeLevel.L0, new Bounds(0, 0, 0, 10, 10, 10));
        parent.AddChild(child);
        parent.MarkClean();

        parent.RemoveChild(child);

        Assert.True(parent.IsDirty);
    }

    #endregion

    #region 状态管理测试

    [Fact]
    public void MarkDirty_SetsIsDirtyTrue()
    {
        var node = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));

        node.MarkDirty();

        Assert.True(node.IsDirty);
    }

    [Fact]
    public void MarkClean_SetsIsDirtyFalse()
    {
        var node = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));
        node.MarkDirty();

        node.MarkClean();

        Assert.False(node.IsDirty);
    }

    [Fact]
    public void SetCollapsed_ChangesStateToCollapsed()
    {
        var node = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));

        node.SetCollapsed();

        Assert.Equal(CollapseState.Collapsed, node.CollapseState);
    }

    #endregion

    #region UpdateHistoryHash 测试

    [Fact]
    public void UpdateHistoryHash_WithNoChildren_InvalidatesCache()
    {
        var node = new HChunkNode(1, NodeLevel.L0, new Bounds(0, 0, 0, 1, 1, 1));

        node.UpdateHistoryHash();

        Assert.False(node.IsDirty);
    }

    [Fact]
    public void UpdateHistoryHash_WithChildren_InvalidatesCache()
    {
        var parent = new HChunkNode(1, NodeLevel.L1, new Bounds(0, 0, 0, 100, 100, 100));
        var child = new HChunkNode(2, NodeLevel.L0, new Bounds(0, 0, 0, 10, 10, 10));
        parent.AddChild(child);
        parent.MarkClean();

        parent.UpdateHistoryHash();

        Assert.False(parent.IsDirty);
    }

    #endregion
}

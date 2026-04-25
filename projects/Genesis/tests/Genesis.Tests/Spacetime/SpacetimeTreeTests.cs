using Genesis.Core;
using Genesis.Spacetime;
using Xunit;

namespace Genesis.Tests.Spacetime;

public class SpacetimeTreeTests
{
    #region 辅助方法

    private static HChunkNode CreateNode(ulong hash, NodeLevel level, Bounds bounds)
    {
        return new HChunkNode(hash, level, bounds);
    }

    private static HChunkNode CreateL3Root()
    {
        return CreateNode(1000, NodeLevel.L3, new Bounds(0, 0, 0, 1000, 1000, 1000));
    }

    private static HChunkNode CreateL2Node(ulong hash, Bounds bounds)
    {
        return CreateNode(hash, NodeLevel.L2, bounds);
    }

    private static HChunkNode CreateL1Node(ulong hash, Bounds bounds)
    {
        return CreateNode(hash, NodeLevel.L1, bounds);
    }

    private static HChunkNode CreateL0Node(ulong hash, Bounds bounds)
    {
        return CreateNode(hash, NodeLevel.L0, bounds);
    }

    #endregion

    #region 构造函数测试

    [Fact]
    public void Constructor_CreatesEmptyTree()
    {
        var tree = new SpacetimeTree();

        Assert.Null(tree.Root);
    }

    #endregion

    #region InsertNode 测试

    [Fact]
    public void InsertNode_L3Node_BecomesRoot()
    {
        var tree = new SpacetimeTree();
        var root = CreateL3Root();

        tree.InsertNode(root);

        Assert.NotNull(tree.Root);
        Assert.Equal(root.SpatialHash, tree.Root.SpatialHash);
    }

    [Fact]
    public void InsertNode_L2Node_AttachesToL3Parent()
    {
        var tree = new SpacetimeTree();
        var root = CreateL3Root();
        tree.InsertNode(root);

        var l2 = CreateL2Node(2001, new Bounds(0, 0, 0, 500, 500, 500));
        tree.InsertNode(l2);

        var found = tree.GetNode(2001);
        Assert.NotNull(found);
        Assert.Equal(NodeLevel.L2, found.Level);
    }

    [Fact]
    public void InsertNode_MultipleNodes_AllIndexed()
    {
        var tree = new SpacetimeTree();
        var root = CreateL3Root();
        tree.InsertNode(root);

        var l2a = CreateL2Node(2001, new Bounds(0, 0, 0, 500, 500, 500));
        var l2b = CreateL2Node(2002, new Bounds(500, 0, 0, 1000, 500, 500));
        tree.InsertNode(l2a);
        tree.InsertNode(l2b);

        Assert.NotNull(tree.GetNode(2001));
        Assert.NotNull(tree.GetNode(2002));
    }

    #endregion

    #region GetNode 测试

    [Fact]
    public void GetNode_ExistingNode_ReturnsNode()
    {
        var tree = new SpacetimeTree();
        var root = CreateL3Root();
        tree.InsertNode(root);

        var result = tree.GetNode(root.SpatialHash);

        Assert.NotNull(result);
        Assert.Equal(root.SpatialHash, result.SpatialHash);
    }

    [Fact]
    public void GetNode_NonExistingNode_ReturnsNull()
    {
        var tree = new SpacetimeTree();

        var result = tree.GetNode(99999);

        Assert.Null(result);
    }

    #endregion

    #region FindNode 测试

    [Fact]
    public void FindNode_EmptyTree_ReturnsNull()
    {
        var tree = new SpacetimeTree();

        var result = tree.FindNode(new Position(5, 5, 5), NodeLevel.L0);

        Assert.Null(result);
    }

    [Fact]
    public void FindNode_PositionInRootBounds_ReturnsRoot()
    {
        var tree = new SpacetimeTree();
        var root = CreateL3Root();
        tree.InsertNode(root);

        var result = tree.FindNode(new Position(500, 500, 500), NodeLevel.L3);

        Assert.NotNull(result);
        Assert.Equal(NodeLevel.L3, result.Level);
    }

    [Fact]
    public void FindNode_PositionOutsideBounds_ReturnsNull()
    {
        var tree = new SpacetimeTree();
        var root = CreateL3Root();
        tree.InsertNode(root);

        var result = tree.FindNode(new Position(2000, 2000, 2000), NodeLevel.L3);

        Assert.Null(result);
    }

    #endregion

    #region RemoveNode 测试

    [Fact]
    public void RemoveNode_RootNode_ClearsRoot()
    {
        var tree = new SpacetimeTree();
        var root = CreateL3Root();
        tree.InsertNode(root);

        tree.RemoveNode(root.SpatialHash);

        Assert.Null(tree.Root);
    }

    [Fact]
    public void RemoveNode_NonExistingNode_DoesNothing()
    {
        var tree = new SpacetimeTree();

        tree.RemoveNode(99999);

        Assert.Null(tree.Root);
    }

    [Fact]
    public void RemoveNode_ChildNode_RemovedFromIndex()
    {
        var tree = new SpacetimeTree();
        var root = CreateL3Root();
        tree.InsertNode(root);

        var l2 = CreateL2Node(2001, new Bounds(0, 0, 0, 500, 500, 500));
        tree.InsertNode(l2);

        tree.RemoveNode(2001);

        Assert.Null(tree.GetNode(2001));
    }

    #endregion

    #region UpdateHistoryHash 测试

    [Fact]
    public void UpdateHistoryHash_ExistingNode_UpdatesHash()
    {
        var tree = new SpacetimeTree();
        var root = CreateL3Root();
        tree.InsertNode(root);

        var originalHash = root.HistoryHash;
        tree.UpdateHistoryHash(root.SpatialHash);

        Assert.NotEqual(originalHash, root.HistoryHash);
    }

    [Fact]
    public void UpdateHistoryHash_NonExistingNode_DoesNothing()
    {
        var tree = new SpacetimeTree();

        tree.UpdateHistoryHash(99999);

        Assert.Null(tree.Root);
    }

    #endregion
}

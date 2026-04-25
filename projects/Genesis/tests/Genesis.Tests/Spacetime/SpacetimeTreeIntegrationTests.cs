using Genesis.Core;
using Genesis.Spacetime;
using Xunit;

namespace Genesis.Tests.Spacetime;

public class SpacetimeTreeIntegrationTests
{
    #region 辅助方法

    private static SpacetimeTree BuildFourLevelTree()
    {
        var tree = new SpacetimeTree();

        var root = new HChunkNode(1000, NodeLevel.L3, new Bounds(0, 0, 0, 1000, 1000, 1000));
        tree.InsertNode(root);

        var l2a = new HChunkNode(2001, NodeLevel.L2, new Bounds(0, 0, 0, 500, 500, 500));
        var l2b = new HChunkNode(2002, NodeLevel.L2, new Bounds(500, 0, 0, 1000, 500, 500));
        tree.InsertNode(l2a);
        tree.InsertNode(l2b);

        var l1a = new HChunkNode(3001, NodeLevel.L1, new Bounds(0, 0, 0, 250, 250, 250));
        var l1b = new HChunkNode(3002, NodeLevel.L1, new Bounds(250, 0, 0, 500, 250, 250));
        tree.InsertNode(l1a);
        tree.InsertNode(l1b);

        var l0a = new HChunkNode(4001, NodeLevel.L0, new Bounds(0, 0, 0, 125, 125, 125));
        var l0b = new HChunkNode(4002, NodeLevel.L0, new Bounds(125, 0, 0, 250, 125, 125));
        tree.InsertNode(l0a);
        tree.InsertNode(l0b);

        return tree;
    }

    #endregion

    #region 四级树构建测试

    [Fact]
    public void FourLevelTree_RootIsL3()
    {
        var tree = BuildFourLevelTree();

        Assert.NotNull(tree.Root);
        Assert.Equal(NodeLevel.L3, tree.Root.Level);
    }

    [Fact]
    public void FourLevelTree_AllNodesIndexed()
    {
        var tree = BuildFourLevelTree();

        Assert.NotNull(tree.GetNode(1000));
        Assert.NotNull(tree.GetNode(2001));
        Assert.NotNull(tree.GetNode(2002));
        Assert.NotNull(tree.GetNode(3001));
        Assert.NotNull(tree.GetNode(3002));
        Assert.NotNull(tree.GetNode(4001));
        Assert.NotNull(tree.GetNode(4002));
    }

    [Fact]
    public void FourLevelTree_RootHasTwoChildren()
    {
        var tree = BuildFourLevelTree();

        Assert.Equal(2, tree.Root.Children.Count);
    }

    [Fact]
    public void FourLevelTree_L2HasL1Children()
    {
        var tree = BuildFourLevelTree();

        var l2a = tree.GetNode(2001) as HChunkNode;
        Assert.NotNull(l2a);
        Assert.Equal(2, l2a.Children.Count);
        Assert.Equal(NodeLevel.L1, l2a.Children[0].Level);
    }

    [Fact]
    public void FourLevelTree_L1HasL0Children()
    {
        var tree = BuildFourLevelTree();

        var l1a = tree.GetNode(3001) as HChunkNode;
        Assert.NotNull(l1a);
        Assert.Equal(2, l1a.Children.Count);
        Assert.Equal(NodeLevel.L0, l1a.Children[0].Level);
    }

    #endregion

    #region FindNode 多层级查找测试

    [Fact]
    public void FindNode_L0PositionInDeepTree()
    {
        var tree = BuildFourLevelTree();

        var result = tree.FindNode(new Position(50, 50, 50), NodeLevel.L0);

        Assert.NotNull(result);
        Assert.Equal(NodeLevel.L0, result.Level);
    }

    [Fact]
    public void FindNode_L1PositionInDeepTree()
    {
        var tree = BuildFourLevelTree();

        var result = tree.FindNode(new Position(100, 100, 100), NodeLevel.L1);

        Assert.NotNull(result);
        Assert.Equal(NodeLevel.L1, result.Level);
    }

    [Fact]
    public void FindNode_L2PositionInDeepTree()
    {
        var tree = BuildFourLevelTree();

        var result = tree.FindNode(new Position(250, 100, 100), NodeLevel.L2);

        Assert.NotNull(result);
        Assert.Equal(NodeLevel.L2, result.Level);
    }

    [Fact]
    public void FindNode_L3PositionInDeepTree()
    {
        var tree = BuildFourLevelTree();

        var result = tree.FindNode(new Position(500, 500, 500), NodeLevel.L3);

        Assert.NotNull(result);
        Assert.Equal(NodeLevel.L3, result.Level);
    }

    [Fact]
    public void FindNode_PositionInSecondL2Branch()
    {
        var tree = BuildFourLevelTree();

        var result = tree.FindNode(new Position(750, 100, 100), NodeLevel.L2);

        Assert.NotNull(result);
        Assert.Equal(NodeLevel.L2, result.Level);
        Assert.Equal(2002UL, result.SpatialHash);
    }

    #endregion

    #region 级联历史哈希更新测试

    [Fact]
    public void UpdateHistoryHash_L0Change_CascadesToRoot()
    {
        var tree = BuildFourLevelTree();

        var l0a = tree.GetNode(4001) as HChunkNode;
        Assert.NotNull(l0a);

        var rootHashBefore = tree.Root.HistoryHash;
        tree.UpdateHistoryHash(4001);
        var rootHashAfter = tree.Root.HistoryHash;

        Assert.NotEqual(rootHashBefore, rootHashAfter);
    }

    [Fact]
    public void UpdateHistoryHash_L1Change_CascadesToRoot()
    {
        var tree = BuildFourLevelTree();

        var rootHashBefore = tree.Root.HistoryHash;
        tree.UpdateHistoryHash(3001);
        var rootHashAfter = tree.Root.HistoryHash;

        Assert.NotEqual(rootHashBefore, rootHashAfter);
    }

    [Fact]
    public void UpdateHistoryHash_L2Change_CascadesToRoot()
    {
        var tree = BuildFourLevelTree();

        var rootHashBefore = tree.Root.HistoryHash;
        tree.UpdateHistoryHash(2001);
        var rootHashAfter = tree.Root.HistoryHash;

        Assert.NotEqual(rootHashBefore, rootHashAfter);
    }

    [Fact]
    public void UpdateHistoryHash_RootChange_DoesNotAffectChildren()
    {
        var tree = BuildFourLevelTree();

        var l2a = tree.GetNode(2001) as HChunkNode;
        Assert.NotNull(l2a);
        var childHashBefore = l2a.HistoryHash;

        tree.UpdateHistoryHash(1000);

        Assert.Equal(childHashBefore, l2a.HistoryHash);
    }

    #endregion

    #region 节点删除与树结构维护测试

    [Fact]
    public void RemoveNode_L0Node_UpdatesParentChildIndex()
    {
        var tree = BuildFourLevelTree();

        var l1a = tree.GetNode(3001) as HChunkNode;
        Assert.NotNull(l1a);
        Assert.Equal(2, l1a.Children.Count);

        tree.RemoveNode(4001);

        Assert.Single(l1a.Children);
    }

    [Fact]
    public void RemoveNode_L2Node_UpdatesRootChildIndex()
    {
        var tree = BuildFourLevelTree();

        Assert.Equal(2, tree.Root.Children.Count);

        tree.RemoveNode(2001);

        Assert.Single(tree.Root.Children);
    }

    [Fact]
    public void RemoveNode_L0Node_CanNoLongerBeFound()
    {
        var tree = BuildFourLevelTree();

        tree.RemoveNode(4001);

        Assert.Null(tree.GetNode(4001));
    }

    [Fact]
    public void RemoveNode_L0Node_SiblingStillAccessible()
    {
        var tree = BuildFourLevelTree();

        tree.RemoveNode(4001);

        Assert.NotNull(tree.GetNode(4002));
    }

    #endregion

    #region 时空树与空间哈希确定性测试

    [Fact]
    public void SpatialHash_Deterministic_SameInputSameOutput()
    {
        var pos = new Position(100, 200, 300);
        var level = NodeLevel.L1;
        var seed = 42UL;

        var hash1 = SpatialHasher.ComputeSpatialHash(pos, level, seed);
        var hash2 = SpatialHasher.ComputeSpatialHash(pos, level, seed);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void SpatialHash_DifferentPosition_DifferentOutput()
    {
        var pos1 = new Position(100, 200, 300);
        var pos2 = new Position(200, 300, 400);
        var seed = 42UL;

        var hash1 = SpatialHasher.ComputeSpatialHash(pos1, NodeLevel.L0, seed);
        var hash2 = SpatialHasher.ComputeSpatialHash(pos2, NodeLevel.L0, seed);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void SpatialHash_DifferentLevel_DifferentOutput()
    {
        var pos = new Position(100, 200, 300);
        var seed = 42UL;

        var hashL0 = SpatialHasher.ComputeSpatialHash(pos, NodeLevel.L0, seed);
        var hashL1 = SpatialHasher.ComputeSpatialHash(pos, NodeLevel.L1, seed);

        Assert.NotEqual(hashL0, hashL1);
    }

    [Fact]
    public void ComputeFromChildren_Deterministic()
    {
        var tree = BuildFourLevelTree();
        var l1a = tree.GetNode(3001) as HChunkNode;
        Assert.NotNull(l1a);

        var hash1 = SpatialHasher.ComputeFromChildren(l1a.Children);
        var hash2 = SpatialHasher.ComputeFromChildren(l1a.Children);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeFromChildren_EmptyList_ReturnsZero()
    {
        var hash = SpatialHasher.ComputeFromChildren([]);

        Assert.Equal(0UL, hash);
    }

    #endregion

    #region HChunkNode 与 WorldSeed 测试

    [Fact]
    public void HChunkNode_WithWorldSeed_HashDeterministic()
    {
        var bounds = new Bounds(0, 0, 0, 100, 100, 100);
        var seed = 42UL;

        var node1 = new HChunkNode(0, NodeLevel.L1, bounds, seed);
        var node2 = new HChunkNode(0, NodeLevel.L1, bounds, seed);

        Assert.Equal(node1.SpatialHash, node2.SpatialHash);
    }

    [Fact]
    public void HChunkNode_WithWorldSeed_DifferentSeedsDifferentHashes()
    {
        var bounds = new Bounds(0, 0, 0, 100, 100, 100);

        var node1 = new HChunkNode(0, NodeLevel.L1, bounds, 42);
        var node2 = new HChunkNode(0, NodeLevel.L1, bounds, 99);

        Assert.NotEqual(node1.SpatialHash, node2.SpatialHash);
    }

    #endregion

    #region 历史哈希链确定性测试

    [Fact]
    public void HistoryHashChain_UpdateIsDeterministic()
    {
        var chain1 = new HistoryHashChain();
        var chain2 = new HistoryHashChain();

        chain1.Update(42);
        chain2.Update(42);

        Assert.Equal(chain1.Value, chain2.Value);
    }

    [Fact]
    public void HistoryHashChain_CombineIsDeterministic()
    {
        var chain1 = new HistoryHashChain();
        var chain2 = new HistoryHashChain();

        chain1.Update(100);
        chain2.Update(100);

        chain1.Combine(200);
        chain2.Combine(200);

        Assert.Equal(chain1.Value, chain2.Value);
    }

    [Fact]
    public void HistoryHashChain_DifferentUpdates_DifferentValues()
    {
        var chain1 = new HistoryHashChain();
        var chain2 = new HistoryHashChain();

        chain1.Update(42);
        chain2.Update(99);

        Assert.NotEqual(chain1.Value, chain2.Value);
    }

    #endregion
}

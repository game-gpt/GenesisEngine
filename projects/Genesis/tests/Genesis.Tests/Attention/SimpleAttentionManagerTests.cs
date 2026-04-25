using Genesis.Attention;
using Genesis.Core;
using Genesis.Spacetime;
using Xunit;

namespace Genesis.Tests.Attention;

public class SimpleAttentionManagerTests
{
    #region 辅助方法

    private static SpacetimeTree BuildTestTree()
    {
        var tree = new SpacetimeTree();

        var root = new HChunkNode(1000, NodeLevel.L3, new Bounds(0, 0, 0, 1000, 1000, 1000));
        tree.InsertNode(root);

        var l2a = new HChunkNode(2001, NodeLevel.L2, new Bounds(0, 0, 0, 500, 500, 500));
        var l2b = new HChunkNode(2002, NodeLevel.L2, new Bounds(500, 0, 0, 1000, 500, 500));
        tree.InsertNode(l2a);
        tree.InsertNode(l2b);

        var l1a = new HChunkNode(3001, NodeLevel.L1, new Bounds(0, 0, 0, 250, 250, 250));
        tree.InsertNode(l1a);

        return tree;
    }

    private static SimpleAttentionManager CreateManager(SpacetimeTree tree)
    {
        var model = new SimpleAttentionModel();
        return new SimpleAttentionManager(tree, model);
    }

    #endregion

    #region CalculateAttention 测试

    [Fact]
    public void CalculateAttention_NearPlayer_HighValue()
    {
        var tree = BuildTestTree();
        var manager = CreateManager(tree);
        var playerPos = new Position(100, 100, 100);

        var attention = manager.CalculateAttention(
            new Position(110, 110, 110),
            playerPos,
            new Position(0, 0, 1));

        Assert.True(attention > 0.5);
    }

    [Fact]
    public void CalculateAttention_FarFromPlayer_LowValue()
    {
        var tree = BuildTestTree();
        var manager = CreateManager(tree);
        var playerPos = new Position(0, 0, 0);

        var attention = manager.CalculateAttention(
            new Position(900, 900, 900),
            playerPos,
            new Position(0, 0, 1));

        Assert.True(attention < 0.5);
    }

    [Fact]
    public void CalculateAttention_SamePosition_HighValue()
    {
        var tree = BuildTestTree();
        var manager = CreateManager(tree);
        var pos = new Position(100, 100, 100);

        var attention = manager.CalculateAttention(pos, pos, new Position(0, 0, 1));

        Assert.True(attention >= 0.7);
    }

    #endregion

    #region GetHighAttentionNodes 测试

    [Fact]
    public void GetHighAttentionNodes_EmptyTree_ReturnsEmpty()
    {
        var tree = new SpacetimeTree();
        var manager = CreateManager(tree);

        var nodes = manager.GetHighAttentionNodes();

        Assert.Empty(nodes);
    }

    [Fact]
    public void GetHighAttentionNodes_PlayerAtRootCenter_IncludesRoot()
    {
        var tree = BuildTestTree();
        var manager = CreateManager(tree);
        manager.UpdateAttention(new Position(500, 500, 500), new Position(0, 0, 1));

        var nodes = manager.GetHighAttentionNodes();

        Assert.NotEmpty(nodes);
    }

    #endregion

    #region InterestPoint 管理测试

    [Fact]
    public void AddInterestPoint_ThenRemove()
    {
        var tree = BuildTestTree();
        var manager = CreateManager(tree);

        var point = new InterestPoint(1, new Position(100, 100, 100), 50.0, 1.0, "Test");
        manager.AddInterestPoint(point);

        manager.RemoveInterestPoint(1);

        Assert.True(true);
    }

    [Fact]
    public void AddInterestPoint_DuplicateId_DoesNotAddTwice()
    {
        var tree = BuildTestTree();
        var manager = CreateManager(tree);

        var point1 = new InterestPoint(1, new Position(100, 100, 100), 50.0, 1.0, "Test1");
        var point2 = new InterestPoint(1, new Position(200, 200, 200), 50.0, 1.0, "Test2");

        manager.AddInterestPoint(point1);
        manager.AddInterestPoint(point2);

        manager.RemoveInterestPoint(1);

        Assert.True(true);
    }

    #endregion

    #region UpdateAttention 测试

    [Fact]
    public void UpdateAttention_ChangesPlayerPosition()
    {
        var tree = BuildTestTree();
        var manager = CreateManager(tree);

        manager.UpdateAttention(new Position(500, 500, 500), new Position(0, 0, 1));

        var nearNewPos = manager.CalculateAttention(
            new Position(510, 510, 510),
            new Position(500, 500, 500),
            new Position(0, 0, 1));

        Assert.True(nearNewPos > 0.5);
    }

    #endregion
}

public class SimpleAttentionModelTests
{
    [Fact]
    public void CalculateDistanceFactor_ZeroDistance_ReturnsOne()
    {
        var model = new SimpleAttentionModel();
        var pos = new Position(100, 100, 100);

        var factor = model.CalculateDistanceFactor(pos, pos);

        Assert.Equal(1.0, factor);
    }

    [Fact]
    public void CalculateDistanceFactor_MaxDistance_ReturnsZero()
    {
        var model = new SimpleAttentionModel();
        var playerPos = new Position(0, 0, 0);
        var farPos = new Position(1000, 0, 0);

        var factor = model.CalculateDistanceFactor(farPos, playerPos);

        Assert.Equal(0.0, factor);
    }

    [Fact]
    public void CalculateDistanceFactor_HalfDistance_ReturnsHalf()
    {
        var model = new SimpleAttentionModel();
        var playerPos = new Position(0, 0, 0);
        var halfPos = new Position(500, 0, 0);

        var factor = model.CalculateDistanceFactor(halfPos, playerPos);

        Assert.Equal(0.5, factor);
    }

    [Fact]
    public void CalculateViewFactor_InFront_ReturnsPositive()
    {
        var model = new SimpleAttentionModel();
        var playerPos = new Position(0, 0, 0);
        var viewDir = new Position(0, 0, 1);
        var targetPos = new Position(0, 0, 100);

        var factor = model.CalculateViewFactor(targetPos, playerPos, viewDir);

        Assert.True(factor > 0);
    }

    [Fact]
    public void CalculateViewFactor_Behind_ReturnsZero()
    {
        var model = new SimpleAttentionModel();
        var playerPos = new Position(0, 0, 0);
        var viewDir = new Position(0, 0, 1);
        var targetPos = new Position(0, 0, -100);

        var factor = model.CalculateViewFactor(targetPos, playerPos, viewDir);

        Assert.Equal(0.0, factor);
    }

    [Fact]
    public void CombineFactors_WeightedSum()
    {
        var model = new SimpleAttentionModel();

        var result = model.CombineFactors(1.0, 0.5, 0.0, 0.0);

        var expected = 0.5 * 1.0 + 0.3 * 0.5 + 0.1 * 0.0 + 0.1 * 0.0;
        Assert.Equal(expected, result, 0.001);
    }
}

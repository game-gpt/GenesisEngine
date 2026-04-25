using Genesis.Core;
using Xunit;

namespace Genesis.Tests.Core;

public class PositionTests
{
    [Fact]
    public void DistanceTo_SamePosition_ReturnsZero()
    {
        var position = new Position(1.0, 2.0, 3.0);

        var distance = position.DistanceTo(position);

        Assert.Equal(0.0, distance);
    }

    [Fact]
    public void DistanceToSquared_SamePosition_ReturnsZero()
    {
        var position = new Position(5.0, -3.0, 7.0);

        var distanceSq = position.DistanceToSquared(position);

        Assert.Equal(0.0, distanceSq);
    }

    [Theory]
    [InlineData(0, 0, 0, 3, 4, 0, 5)]
    [InlineData(0, 0, 0, 1, 1, 1, 1.732)]
    [InlineData(-1, -1, -1, 2, 2, 2, 5.196)]
    public void DistanceTo_TwoPositions_ReturnsCorrectDistance(
        double x1, double y1, double z1,
        double x2, double y2, double z2,
        double expected)
    {
        var p1 = new Position(x1, y1, z1);
        var p2 = new Position(x2, y2, z2);

        var distance = p1.DistanceTo(p2);

        Assert.Equal(expected, distance, 3);
    }

    [Fact]
    public void DistanceToSquared_TwoPositions_ReturnsCorrectValue()
    {
        var p1 = new Position(0, 0, 0);
        var p2 = new Position(3, 4, 0);

        var distanceSq = p1.DistanceToSquared(p2);

        Assert.Equal(25.0, distanceSq);
    }

    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        var p1 = new Position(1.0, 2.0, 3.0);
        var p2 = new Position(4.0, 5.0, 6.0);

        var d1 = p1.DistanceTo(p2);
        var d2 = p2.DistanceTo(p1);

        Assert.Equal(d1, d2, 10);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var p1 = new Position(1.0, 2.0, 3.0);
        var p2 = new Position(1.0, 2.0, 3.0);

        Assert.Equal(p1, p2);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var p1 = new Position(1.0, 2.0, 3.0);
        var p2 = new Position(1.0, 2.0, 4.0);

        Assert.NotEqual(p1, p2);
    }
}

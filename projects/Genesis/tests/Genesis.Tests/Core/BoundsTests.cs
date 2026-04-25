using Genesis.Core;
using Xunit;

namespace Genesis.Tests.Core;

public class BoundsTests
{
    [Fact]
    public void Center_ReturnsCenterPosition()
    {
        var bounds = new Bounds(0, 0, 0, 10, 20, 30);

        var center = bounds.Center;

        Assert.Equal(new Position(5, 10, 15), center);
    }

    [Fact]
    public void SizeX_ReturnsCorrectSize()
    {
        var bounds = new Bounds(0, 0, 0, 10, 20, 30);

        Assert.Equal(10.0, bounds.SizeX);
    }

    [Fact]
    public void SizeY_ReturnsCorrectSize()
    {
        var bounds = new Bounds(0, 0, 0, 10, 20, 30);

        Assert.Equal(20.0, bounds.SizeY);
    }

    [Fact]
    public void SizeZ_ReturnsCorrectSize()
    {
        var bounds = new Bounds(0, 0, 0, 10, 20, 30);

        Assert.Equal(30.0, bounds.SizeZ);
    }

    [Fact]
    public void Contains_PointInside_ReturnsTrue()
    {
        var bounds = new Bounds(0, 0, 0, 10, 10, 10);
        var position = new Position(5, 5, 5);

        Assert.True(bounds.Contains(position));
    }

    [Fact]
    public void Contains_PointOutside_ReturnsFalse()
    {
        var bounds = new Bounds(0, 0, 0, 10, 10, 10);
        var position = new Position(15, 5, 5);

        Assert.False(bounds.Contains(position));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(10, 10, 10)]
    public void Contains_PointOnBoundary_ReturnsTrue(double x, double y, double z)
    {
        var bounds = new Bounds(0, 0, 0, 10, 10, 10);
        var position = new Position(x, y, z);

        Assert.True(bounds.Contains(position));
    }

    [Fact]
    public void Contains_PointJustOutside_ReturnsFalse()
    {
        var bounds = new Bounds(0, 0, 0, 10, 10, 10);
        var position = new Position(10.001, 5, 5);

        Assert.False(bounds.Contains(position));
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var b1 = new Bounds(0, 0, 0, 10, 10, 10);
        var b2 = new Bounds(0, 0, 0, 10, 10, 10);

        Assert.Equal(b1, b2);
    }
}

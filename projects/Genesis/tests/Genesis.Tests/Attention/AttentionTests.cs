using Genesis.Attention;
using Genesis.Core;
using Xunit;

namespace Genesis.Tests.Attention;

public class AttentionLevelTests
{
    [Fact]
    public void Zero_ShouldNotCollapse()
    {
        Assert.False(AttentionLevel.Zero.ShouldCollapse);
    }

    [Fact]
    public void Low_ShouldNotCollapse()
    {
        Assert.False(AttentionLevel.Low.ShouldCollapse);
    }

    [Fact]
    public void Medium_ShouldNotCollapse()
    {
        Assert.False(AttentionLevel.Medium.ShouldCollapse);
    }

    [Fact]
    public void High_ShouldCollapse()
    {
        Assert.True(AttentionLevel.High.ShouldCollapse);
    }

    [Fact]
    public void Critical_ShouldCollapse()
    {
        Assert.True(AttentionLevel.Critical.ShouldCollapse);
    }

    [Fact]
    public void ShouldCollapse_ThresholdIsHigh()
    {
        var below = new AttentionLevel(0.74);
        var atThreshold = new AttentionLevel(0.75);
        var above = new AttentionLevel(0.76);

        Assert.False(below.ShouldCollapse);
        Assert.True(atThreshold.ShouldCollapse);
        Assert.True(above.ShouldCollapse);
    }
}

public class InterestPointTests
{
    [Fact]
    public void Contains_PointWithinRadius_ReturnsTrue()
    {
        var point = new InterestPoint(1, new Position(0, 0, 0), 10.0, 1.0, "Player");
        var position = new Position(5, 5, 0);

        Assert.True(point.Contains(position));
    }

    [Fact]
    public void Contains_PointOutsideRadius_ReturnsFalse()
    {
        var point = new InterestPoint(1, new Position(0, 0, 0), 5.0, 1.0, "Player");
        var position = new Position(10, 0, 0);

        Assert.False(point.Contains(position));
    }

    [Fact]
    public void Contains_PointAtExactRadius_ReturnsTrue()
    {
        var point = new InterestPoint(1, new Position(0, 0, 0), 5.0, 1.0, "Player");
        var position = new Position(5, 0, 0);

        Assert.True(point.Contains(position));
    }

    [Fact]
    public void Contains_CenterPosition_ReturnsTrue()
    {
        var center = new Position(10, 20, 30);
        var point = new InterestPoint(1, center, 1.0, 1.0, "NPC");

        Assert.True(point.Contains(center));
    }
}

public class DirtyRegionTests
{
    [Fact]
    public void MarkDirty_SetsIsDirtyTrue()
    {
        var region = new DirtyRegion(1, new Bounds(0, 0, 0, 10, 10, 10), NodeLevel.L0, DateTime.UtcNow, false);

        var dirty = region.MarkDirty();

        Assert.True(dirty.IsDirty);
    }

    [Fact]
    public void MarkDirty_UpdatesLastModified()
    {
        var originalTime = DateTime.UtcNow.AddHours(-1);
        var region = new DirtyRegion(1, new Bounds(0, 0, 0, 10, 10, 10), NodeLevel.L0, originalTime, false);

        var dirty = region.MarkDirty();

        Assert.True(dirty.LastModified > originalTime);
    }

    [Fact]
    public void MarkClean_SetsIsDirtyFalse()
    {
        var region = new DirtyRegion(1, new Bounds(0, 0, 0, 10, 10, 10), NodeLevel.L0, DateTime.UtcNow, true);

        var clean = region.MarkClean();

        Assert.False(clean.IsDirty);
    }

    [Fact]
    public void MarkDirty_DoesNotModifyOriginal()
    {
        var region = new DirtyRegion(1, new Bounds(0, 0, 0, 10, 10, 10), NodeLevel.L0, DateTime.UtcNow, false);

        region.MarkDirty();

        Assert.False(region.IsDirty);
    }
}

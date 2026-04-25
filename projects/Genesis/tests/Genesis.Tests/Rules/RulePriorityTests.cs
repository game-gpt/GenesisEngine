using Genesis.Rules;
using Xunit;

namespace Genesis.Tests.Rules;

public class RulePriorityTests
{
    [Fact]
    public void Lowest_HasValueZero()
    {
        Assert.Equal(0, RulePriority.Lowest.Value);
    }

    [Fact]
    public void Low_HasValue25()
    {
        Assert.Equal(25, RulePriority.Low.Value);
    }

    [Fact]
    public void Normal_HasValue50()
    {
        Assert.Equal(50, RulePriority.Normal.Value);
    }

    [Fact]
    public void High_HasValue75()
    {
        Assert.Equal(75, RulePriority.High.Value);
    }

    [Fact]
    public void Highest_HasValue100()
    {
        Assert.Equal(100, RulePriority.Highest.Value);
    }

    [Fact]
    public void CustomPriority_CanBeCreated()
    {
        var custom = new RulePriority(33);

        Assert.Equal(33, custom.Value);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var p1 = RulePriority.Normal;
        var p2 = new RulePriority(50);

        Assert.Equal(p1, p2);
    }

    [Fact]
    public void Priorities_AreOrdered()
    {
        Assert.True(RulePriority.Lowest.Value < RulePriority.Low.Value);
        Assert.True(RulePriority.Low.Value < RulePriority.Normal.Value);
        Assert.True(RulePriority.Normal.Value < RulePriority.High.Value);
        Assert.True(RulePriority.High.Value < RulePriority.Highest.Value);
    }
}

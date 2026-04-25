using Genesis.Causal;
using Xunit;

namespace Genesis.Tests.Causal;

public class CausalAnchorTests
{
    [Fact]
    public void ApplyDelta_PositiveDelta_IncreasesWeight()
    {
        var anchor = new CausalAnchor("a1", "TestAnchor", 1.0, new Dictionary<string, double>());

        var result = anchor.ApplyDelta(0.5);

        Assert.Equal(1.5, result.Weight);
    }

    [Fact]
    public void ApplyDelta_NegativeDelta_DecreasesWeight()
    {
        var anchor = new CausalAnchor("a1", "TestAnchor", 1.0, new Dictionary<string, double>());

        var result = anchor.ApplyDelta(-0.3);

        Assert.Equal(0.7, result.Weight);
    }

    [Fact]
    public void ApplyDelta_DoesNotModifyOriginal()
    {
        var anchor = new CausalAnchor("a1", "TestAnchor", 1.0, new Dictionary<string, double>());

        anchor.ApplyDelta(0.5);

        Assert.Equal(1.0, anchor.Weight);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var effects = new Dictionary<string, double> { ["x"] = 1.0 };
        var a1 = new CausalAnchor("a1", "Test", 1.0, effects);
        var a2 = new CausalAnchor("a1", "Test", 1.0, effects);

        Assert.Equal(a1, a2);
    }

    [Fact]
    public void RecordEquality_DifferentId_AreNotEqual()
    {
        var a1 = new CausalAnchor("a1", "Test", 1.0, new Dictionary<string, double>());
        var a2 = new CausalAnchor("a2", "Test", 1.0, new Dictionary<string, double>());

        Assert.NotEqual(a1, a2);
    }
}

public class CausalWeightTests
{
    [Fact]
    public void Add_IncreasesValue()
    {
        var weight = new CausalWeight("Hope", 0.5);

        var result = weight.Add(0.3);

        Assert.Equal(0.8, result.Value);
    }

    [Fact]
    public void Add_PreservesDimension()
    {
        var weight = new CausalWeight("Hope", 0.5);

        var result = weight.Add(0.3);

        Assert.Equal("Hope", result.Dimension);
    }

    [Fact]
    public void Clamp_LimitsValue()
    {
        var weight = new CausalWeight("Hope", 1.5);

        var result = weight.Clamp(0.0, 1.0);

        Assert.Equal(1.0, result.Value);
    }

    [Fact]
    public void Clamp_BelowMin_ClampsToMin()
    {
        var weight = new CausalWeight("Hope", -0.5);

        var result = weight.Clamp(0.0, 1.0);

        Assert.Equal(0.0, result.Value);
    }

    [Fact]
    public void Clamp_WithinRange_DoesNotChange()
    {
        var weight = new CausalWeight("Hope", 0.5);

        var result = weight.Clamp(0.0, 1.0);

        Assert.Equal(0.5, result.Value);
    }

    [Fact]
    public void Clamp_PreservesDimension()
    {
        var weight = new CausalWeight("Chaos", 1.5);

        var result = weight.Clamp(0.0, 1.0);

        Assert.Equal("Chaos", result.Dimension);
    }
}

using Genesis.Core;
using Genesis.Collapse;
using Xunit;

namespace Genesis.Tests.Collapse;

public class WaveFunctionStateTests
{
    [Fact]
    public void CalculateEntropy_SingleBitMask_ReturnsZero()
    {
        var state = new WaveFunctionState(1, CollapseState.Superposition, 0, [1UL]);

        var entropy = state.CalculateEntropy(0);

        Assert.Equal(0.0, entropy);
    }

    [Fact]
    public void CalculateEntropy_TwoBitMask_ReturnsOne()
    {
        var state = new WaveFunctionState(1, CollapseState.Superposition, 0, [3UL]);

        var entropy = state.CalculateEntropy(0);

        Assert.Equal(1.0, entropy);
    }

    [Fact]
    public void CalculateEntropy_FourBitMask_ReturnsTwo()
    {
        var state = new WaveFunctionState(1, CollapseState.Superposition, 0, [15UL]);

        var entropy = state.CalculateEntropy(0);

        Assert.Equal(2.0, entropy);
    }

    [Fact]
    public void CalculateEntropy_EightBitMask_ReturnsThree()
    {
        var state = new WaveFunctionState(1, CollapseState.Superposition, 0, [255UL]);

        var entropy = state.CalculateEntropy(0);

        Assert.Equal(3.0, entropy);
    }

    [Fact]
    public void CalculateEntropy_ZeroMask_ReturnsZero()
    {
        var state = new WaveFunctionState(1, CollapseState.Superposition, 0, [0UL]);

        var entropy = state.CalculateEntropy(0);

        Assert.Equal(0.0, entropy);
    }

    [Fact]
    public void CalculateEntropy_MultipleCells_IndependentEntropy()
    {
        var state = new WaveFunctionState(2, CollapseState.Superposition, 0, [3UL, 15UL]);

        var entropy0 = state.CalculateEntropy(0);
        var entropy1 = state.CalculateEntropy(1);

        Assert.Equal(1.0, entropy0);
        Assert.Equal(2.0, entropy1);
    }

    [Fact]
    public void RecordEquality_SameArrayReference_AreEqual()
    {
        var masks = new ulong[] { 3UL, 15UL };
        var s1 = new WaveFunctionState(2, CollapseState.Superposition, 42, masks);
        var s2 = new WaveFunctionState(2, CollapseState.Superposition, 42, masks);

        Assert.Equal(s1, s2);
    }

    [Fact]
    public void RecordEquality_DifferentArrayReference_AreNotEqual()
    {
        var s1 = new WaveFunctionState(2, CollapseState.Superposition, 42, [3UL, 15UL]);
        var s2 = new WaveFunctionState(2, CollapseState.Superposition, 42, [3UL, 15UL]);

        Assert.NotEqual(s1, s2);
    }
}

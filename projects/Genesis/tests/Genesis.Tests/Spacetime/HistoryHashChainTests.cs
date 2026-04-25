using Genesis.Spacetime;
using Xunit;

namespace Genesis.Tests.Spacetime;

public class HistoryHashChainTests
{
    [Fact]
    public void Value_InitiallyZero()
    {
        var chain = new HistoryHashChain();

        Assert.Equal(0UL, chain.Value);
    }

    [Fact]
    public void Update_SetsNewValue()
    {
        var chain = new HistoryHashChain();

        chain.Update(42);

        Assert.Equal(42UL, chain.Value);
    }

    [Fact]
    public void Update_OverwritesPreviousValue()
    {
        var chain = new HistoryHashChain();
        chain.Update(42);

        chain.Update(100);

        Assert.Equal(100UL, chain.Value);
    }

    [Fact]
    public void Combine_ReturnsCombinedValue()
    {
        var chain = new HistoryHashChain();
        chain.Update(100);

        var result = chain.Combine(200);

        Assert.NotEqual(100UL, result);
        Assert.NotEqual(200UL, result);
        Assert.Equal(chain.Value, result);
    }

    [Fact]
    public void Combine_IsDeterministic()
    {
        var chain1 = new HistoryHashChain();
        chain1.Update(100);
        var result1 = chain1.Combine(200);

        var chain2 = new HistoryHashChain();
        chain2.Update(100);
        var result2 = chain2.Combine(200);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Combine_DifferentInputs_DifferentResults()
    {
        var chain1 = new HistoryHashChain();
        chain1.Update(100);
        var result1 = chain1.Combine(200);

        var chain2 = new HistoryHashChain();
        chain2.Update(100);
        var result2 = chain2.Combine(300);

        Assert.NotEqual(result1, result2);
    }
}

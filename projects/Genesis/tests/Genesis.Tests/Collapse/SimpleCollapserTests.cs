using Genesis.Core;
using Genesis.Collapse;
using Xunit;

namespace Genesis.Tests.Collapse;

public class SimpleCollapserTests
{
    #region CreateState 测试

    [Fact]
    public void CreateState_ReturnsMutableState()
    {
        var collapser = new SimpleCollapser();

        var state = collapser.CreateState(4, 42);

        Assert.Equal(4, state.CellCount);
        Assert.Equal(CollapseState.Superposition, state.State);
    }

    [Fact]
    public void CreateState_AllCellsStartSuperposition()
    {
        var collapser = new SimpleCollapser();

        var state = collapser.CreateState(3, 42);

        for (var i = 0; i < state.CellCount; i++)
        {
            Assert.True(state.CalculateEntropy(i) > 0);
        }
    }

    #endregion

    #region Collapse 测试

    [Fact]
    public void Collapse_SingleCellWithConstraint_ReducesEntropy()
    {
        var collapser = new SimpleCollapser();
        var state = collapser.CreateState(1, 0);
        var constraints = new List<IConstraint>
        {
            new SimpleConstraint("AllowLow", 0, 0xF)
        };

        var result = collapser.Collapse(state, constraints);

        Assert.True(result);
        Assert.Equal(0.0, state.CalculateEntropy(0));
    }

    [Fact]
    public void Collapse_MultipleCells_CollapsesOneAtATime()
    {
        var collapser = new SimpleCollapser();
        var state = collapser.CreateState(3, 0);
        var constraints = new List<IConstraint>
        {
            new SimpleConstraint("AllowLow", 0, 0xF)
        };

        collapser.Collapse(state, constraints);

        var collapsedCount = 0;
        for (var i = 0; i < state.CellCount; i++)
        {
            if (state.CalculateEntropy(i) == 0)
            {
                collapsedCount++;
            }
        }

        Assert.Equal(1, collapsedCount);
    }

    [Fact]
    public void Collapse_ConflictingConstraints_ReturnsFalse()
    {
        var collapser = new SimpleCollapser();
        var state = collapser.CreateState(1, 0);
        state.SetCellMask(0, 0xF0);
        var constraints = new List<IConstraint>
        {
            new SimpleConstraint("AllowLow", 0, 0x0F)
        };

        var result = collapser.Collapse(state, constraints);

        Assert.False(result);
    }

    [Fact]
    public void Collapse_FullyCollapsedState_ReturnsFalse()
    {
        var collapser = new SimpleCollapser();
        var state = collapser.CreateState(1, 0);
        state.SetCellMask(0, 1);

        var constraints = new List<IConstraint>
        {
            new SimpleConstraint("AllowLow", 0, 0xF)
        };

        var result = collapser.Collapse(state, constraints);

        Assert.False(result);
    }

    #endregion

    #region IsFullyCollapsed 测试

    [Fact]
    public void IsFullyCollapsed_AllCollapsed_ReturnsTrue()
    {
        var collapser = new SimpleCollapser();
        var state = collapser.CreateState(2, 0);
        state.SetCellMask(0, 1);
        state.SetCellMask(1, 2);

        Assert.True(collapser.IsFullyCollapsed(state));
    }

    [Fact]
    public void IsFullyCollapsed_SomeCollapsed_ReturnsFalse()
    {
        var collapser = new SimpleCollapser();
        var state = collapser.CreateState(2, 0);
        state.SetCellMask(0, 1);

        Assert.False(collapser.IsFullyCollapsed(state));
    }

    [Fact]
    public void IsFullyCollapsed_NoneCollapsed_ReturnsFalse()
    {
        var collapser = new SimpleCollapser();
        var state = collapser.CreateState(3, 42);

        Assert.False(collapser.IsFullyCollapsed(state));
    }

    #endregion

    #region 约束求解集成测试

    [Fact]
    public void FullCollapsePipeline_CollapsesAllCells()
    {
        var collapser = new SimpleCollapser();
        var state = collapser.CreateState(4, 42);
        var constraints = new List<IConstraint>
        {
            new SimpleConstraint("AllowLow", 0, 0xFF)
        };

        for (var i = 0; i < state.CellCount; i++)
        {
            if (!collapser.IsFullyCollapsed(state))
            {
                collapser.Collapse(state, constraints);
            }
        }

        Assert.True(collapser.IsFullyCollapsed(state));
    }

    [Fact]
    public void MultipleConstraints_AreAppliedInPriorityOrder()
    {
        var collapser = new SimpleCollapser();
        var state = collapser.CreateState(1, 0);
        var constraints = new List<IConstraint>
        {
            new SimpleConstraint("Broad", 10, 0xFF),
            new SimpleConstraint("Narrow", 0, 0x0F)
        };

        collapser.Collapse(state, constraints);

        var mask = state.GetCellMask(0);
        Assert.True(PopCount(mask) == 1);
        Assert.True(mask <= 0x0F);
    }

    #endregion

    #region 辅助方法

    private static int PopCount(ulong value)
    {
        var count = 0;
        while (value != 0)
        {
            count++;
            value &= value - 1;
        }

        return count;
    }

    #endregion
}

public class SimpleConstraintTests
{
    [Fact]
    public void Validate_AllowedMaskOverlap_ReturnsTrue()
    {
        var constraint = new SimpleConstraint("Test", 0, 0xFF);

        var result = constraint.Validate(0, 0x0F, null!);

        Assert.True(result);
    }

    [Fact]
    public void Validate_NoOverlap_ReturnsFalse()
    {
        var constraint = new SimpleConstraint("Test", 0, 0x0F);

        var result = constraint.Validate(0, 0xF0, null!);

        Assert.False(result);
    }

    [Fact]
    public void Apply_RestrictsMaskToAllowedBits()
    {
        var constraint = new SimpleConstraint("Test", 0, 0x0F);

        var result = constraint.Apply(0, 0xFF, null!);

        Assert.Equal(0x0FUL, result);
    }

    [Fact]
    public void Apply_NoOverlap_ReturnsZero()
    {
        var constraint = new SimpleConstraint("Test", 0, 0x0F);

        var result = constraint.Apply(0, 0xF0, null!);

        Assert.Equal(0UL, result);
    }
}

public class MutableWaveFunctionStateTests
{
    [Fact]
    public void FindMinEntropyCell_ReturnsCellWithLowestNonZeroEntropy()
    {
        var state = new MutableWaveFunctionState(3, 0);
        state.SetCellMask(0, 0x3);
        state.SetCellMask(1, 0xFF);
        state.SetCellMask(2, 0xF);

        var minCell = state.FindMinEntropyCell();

        Assert.Equal(0, minCell);
    }

    [Fact]
    public void FindMinEntropyCell_AllCollapsed_ReturnsNegativeOne()
    {
        var state = new MutableWaveFunctionState(2, 0);
        state.SetCellMask(0, 1);
        state.SetCellMask(1, 2);

        var minCell = state.FindMinEntropyCell();

        Assert.Equal(-1, minCell);
    }

    [Fact]
    public void SetCellMask_SingleBit_TransitionsToCollapsed()
    {
        var state = new MutableWaveFunctionState(1, 0);

        state.SetCellMask(0, 1);

        Assert.Equal(CollapseState.Collapsed, state.State);
    }

    [Fact]
    public void SetCellMask_MultipleBits_RemainsSuperposition()
    {
        var state = new MutableWaveFunctionState(2, 0);

        state.SetCellMask(0, 1);

        Assert.Equal(CollapseState.Superposition, state.State);
    }
}

using Genesis.Core;

namespace Genesis.Collapse;

public sealed class SimpleCollapser : ICollapser
{
    #region ICollapser 实现

    public IWaveFunctionState CreateState(int cellCount, ulong seed)
    {
        return new MutableWaveFunctionState(cellCount, seed);
    }

    public bool Collapse(IWaveFunctionState state, IReadOnlyList<IConstraint> constraints)
    {
        var minCell = state.FindMinEntropyCell();
        if (minCell < 0)
        {
            return false;
        }

        var mask = state.GetCellMask(minCell);

        foreach (var constraint in constraints.OrderBy(c => c.Priority))
        {
            mask = constraint.Apply(minCell, mask, state);
        }

        if (mask == 0)
        {
            return false;
        }

        var bitIndex = PickRandomBit(mask, state.CollapseSeed);
        state.SetCellMask(minCell, 1UL << bitIndex);

        return Propagate(state, minCell);
    }

    public bool Propagate(IWaveFunctionState state, int cellIndex)
    {
        return state.GetCellMask(cellIndex) != 0;
    }

    public bool IsFullyCollapsed(IWaveFunctionState state)
    {
        for (var i = 0; i < state.CellCount; i++)
        {
            if (PopCount(state.GetCellMask(i)) > 1)
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region 私有方法

    private static int PickRandomBit(ulong mask, ulong seed)
    {
        var bits = new List<int>();
        for (var i = 0; i < 64; i++)
        {
            if ((mask & (1UL << i)) != 0)
            {
                bits.Add(i);
            }
        }

        var index = (int)(seed % (ulong)bits.Count);
        return bits[index];
    }

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

using Genesis.Core;

namespace Genesis.Collapse;

public sealed class MutableWaveFunctionState : IWaveFunctionState
{
    #region 字段

    private readonly ulong[] _cellMasks;

    #endregion

    #region 属性

    public int CellCount { get; }
    public CollapseState State { get; private set; }
    public ulong CollapseSeed { get; }
    public IReadOnlyList<ulong> CellMasks => Array.AsReadOnly(_cellMasks);

    #endregion

    #region 构造函数

    public MutableWaveFunctionState(int cellCount, ulong seed, ulong initialMask = ulong.MaxValue)
    {
        CellCount = cellCount;
        CollapseSeed = seed;
        State = CollapseState.Superposition;
        _cellMasks = new ulong[cellCount];

        for (var i = 0; i < cellCount; i++)
        {
            _cellMasks[i] = initialMask;
        }
    }

    #endregion

    #region IWaveFunctionState 实现

    public void SetCellMask(int index, ulong mask)
    {
        _cellMasks[index] = mask;
        CheckFullyCollapsed();
    }

    public ulong GetCellMask(int index)
    {
        return _cellMasks[index];
    }

    public double CalculateEntropy(int index)
    {
        var mask = _cellMasks[index];
        var count = PopCount(mask);
        if (count <= 1) return 0;
        return Math.Log2(count);
    }

    public int FindMinEntropyCell()
    {
        var minEntropy = double.MaxValue;
        var minIndex = -1;

        for (var i = 0; i < CellCount; i++)
        {
            var entropy = CalculateEntropy(i);
            if (entropy > 0 && entropy < minEntropy)
            {
                minEntropy = entropy;
                minIndex = i;
            }
        }

        return minIndex;
    }

    #endregion

    #region 私有方法

    private void CheckFullyCollapsed()
    {
        for (var i = 0; i < CellCount; i++)
        {
            if (PopCount(_cellMasks[i]) > 1)
            {
                return;
            }
        }

        State = CollapseState.Collapsed;
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

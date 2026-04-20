using Genesis.Core.Enums;

namespace Genesis.Collapse.ValueObjects;

public readonly record struct WaveFunctionState(
    int CellCount,
    CollapseState State,
    ulong CollapseSeed,
    ulong[] CellMasks)
{
    public double CalculateEntropy(int index)
    {
        var mask = CellMasks[index];
        var count = PopCount(mask);
        if (count <= 1) return 0;
        return Math.Log2(count);
    }

    private static int PopCount(ulong value)
    {
        int count = 0;
        while (value != 0)
        {
            count++;
            value &= value - 1;
        }
        return count;
    }
}

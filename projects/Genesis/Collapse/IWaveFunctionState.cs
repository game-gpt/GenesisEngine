using Genesis.Core;

namespace Genesis.Collapse;

public interface IWaveFunctionState
{
    int CellCount { get; }
    CollapseState State { get; }
    ulong CollapseSeed { get; }
    IReadOnlyList<ulong> CellMasks { get; }
    void SetCellMask(int index, ulong mask);
    ulong GetCellMask(int index);
    double CalculateEntropy(int index);
    int FindMinEntropyCell();
}

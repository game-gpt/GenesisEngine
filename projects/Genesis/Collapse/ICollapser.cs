using Genesis.Core.Enums;
using Genesis.Spacetime.Interfaces;

namespace Genesis.Collapse.Interfaces;

public interface ICollapser
{
    IWaveFunctionState CreateState(int cellCount, ulong seed);
    bool Collapse(IWaveFunctionState state, IReadOnlyList<IConstraint> constraints);
    bool Propagate(IWaveFunctionState state, int cellIndex);
    bool IsFullyCollapsed(IWaveFunctionState state);
}

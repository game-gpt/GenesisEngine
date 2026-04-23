namespace Genesis.Collapse;

public interface ICollapser
{
    IWaveFunctionState CreateState(int cellCount, ulong seed);
    bool Collapse(IWaveFunctionState state, IReadOnlyList<IConstraint> constraints);
    bool Propagate(IWaveFunctionState state, int cellIndex);
    bool IsFullyCollapsed(IWaveFunctionState state);
}

namespace Genesis.Collapse.Interfaces;

public interface IConstraint
{
    string Name { get; }
    int Priority { get; }
    bool Validate(int cellIndex, ulong cellMask, IWaveFunctionState state);
    ulong Apply(int cellIndex, ulong cellMask, IWaveFunctionState state);
}

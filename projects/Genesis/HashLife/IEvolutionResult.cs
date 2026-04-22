using Genesis.Core.ValueObjects;

namespace Genesis.HashLife.Interfaces;

public interface IEvolutionResult
{
    ulong ResultHash { get; }
    Timestamp EvolvedAt { get; }
    float TimeDelta { get; }
    bool IsValid { get; }
}

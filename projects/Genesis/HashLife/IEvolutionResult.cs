using Genesis.Core;

namespace Genesis.HashLife;

public interface IEvolutionResult
{
    ulong ResultHash { get; }
    Timestamp EvolvedAt { get; }
    float TimeDelta { get; }
    bool IsValid { get; }
}

namespace Genesis.Rules.Interfaces;

public interface IEmergentRule : IRule
{
    double CalculateProbability(ulong contextHash);
    void ApplyStatisticalModel(float deltaTime);
    ulong SampleResult(ulong contextHash, ulong seed);
}

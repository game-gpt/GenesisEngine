namespace Genesis.Rules;

public interface IDeterministicRule : IRule
{
    ulong Evolve(ulong inputHash, float deltaTime);
    bool Validate(ulong inputHash, ulong outputHash, float deltaTime);
}

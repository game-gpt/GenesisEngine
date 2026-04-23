using Genesis.Core;

namespace Genesis.Rules;

public interface IRule
{
    string Name { get; }
    RuleType Type { get; }
    int Priority { get; }
    bool IsCacheable { get; }
    void Execute(float deltaTime);
}

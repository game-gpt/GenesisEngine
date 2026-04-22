using Genesis.Core.Enums;

namespace Genesis.Rules.Interfaces;

public interface IRule
{
    string Name { get; }
    RuleType Type { get; }
    int Priority { get; }
    bool IsCacheable { get; }
    void Execute(float deltaTime);
}

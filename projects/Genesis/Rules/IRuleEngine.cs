using Genesis.Core.Enums;

namespace Genesis.Rules.Interfaces;

public interface IRuleEngine
{
    void RegisterRule(IRule rule);
    void UnregisterRule(string ruleName);
    void ExecuteRules(float deltaTime);
    void ExecuteRulesByType(RuleType type, float deltaTime);
    IReadOnlyList<IRule> GetRulesByType(RuleType type);
    IReadOnlyList<IRule> GetRulesByPriority(int minPriority, int maxPriority);
}

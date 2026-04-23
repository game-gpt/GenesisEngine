using Genesis.Core;

namespace Genesis.Rules;

public interface IRuleEngine
{
    void RegisterRule(IRule rule);
    void UnregisterRule(string ruleName);
    void ExecuteRules(float deltaTime);
    void ExecuteRulesByType(RuleType type, float deltaTime);
    IReadOnlyList<IRule> GetRulesByType(RuleType type);
    IReadOnlyList<IRule> GetRulesByPriority(int minPriority, int maxPriority);
}

using Genesis.Core;

namespace Genesis.Rules;

public sealed class SimpleRuleEngine : IRuleEngine
{
    #region 字段

    private readonly List<IRule> _rules;

    #endregion

    #region 构造函数

    public SimpleRuleEngine()
    {
        _rules = new List<IRule>();
    }

    #endregion

    #region IRuleEngine 实现

    public void RegisterRule(IRule rule)
    {
        if (_rules.Any(r => r.Name == rule.Name))
        {
            return;
        }

        _rules.Add(rule);
        _rules.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    public void UnregisterRule(string ruleName)
    {
        _rules.RemoveAll(r => r.Name == ruleName);
    }

    public void ExecuteRules(float deltaTime)
    {
        foreach (var rule in _rules)
        {
            rule.Execute(deltaTime);
        }
    }

    public void ExecuteRulesByType(RuleType type, float deltaTime)
    {
        foreach (var rule in _rules.Where(r => r.Type == type))
        {
            rule.Execute(deltaTime);
        }
    }

    public IReadOnlyList<IRule> GetRulesByType(RuleType type)
    {
        return _rules.Where(r => r.Type == type).ToList().AsReadOnly();
    }

    public IReadOnlyList<IRule> GetRulesByPriority(int minPriority, int maxPriority)
    {
        return _rules
            .Where(r => r.Priority >= minPriority && r.Priority <= maxPriority)
            .ToList()
            .AsReadOnly();
    }

    #endregion
}

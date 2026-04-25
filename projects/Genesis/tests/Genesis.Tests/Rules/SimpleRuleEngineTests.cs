using Genesis.Core;
using Genesis.Rules;
using Xunit;

namespace Genesis.Tests.Rules;

public class SimpleRuleEngineTests
{
    #region 辅助类

    private class TestRule : IRule
    {
        public string Name { get; }
        public RuleType Type { get; }
        public int Priority { get; }
        public bool IsCacheable { get; } = true;
        public int ExecuteCount { get; private set; }

        public TestRule(string name, RuleType type, int priority)
        {
            Name = name;
            Type = type;
            Priority = priority;
        }

        public void Execute(float deltaTime)
        {
            ExecuteCount++;
        }
    }

    #endregion

    #region RegisterRule 测试

    [Fact]
    public void RegisterRule_AddsRule()
    {
        var engine = new SimpleRuleEngine();
        var rule = new TestRule("TestRule", RuleType.Deterministic, 50);

        engine.RegisterRule(rule);

        var rules = engine.GetRulesByType(RuleType.Deterministic);
        Assert.Single(rules);
        Assert.Equal("TestRule", rules[0].Name);
    }

    [Fact]
    public void RegisterRule_DuplicateName_DoesNotAdd()
    {
        var engine = new SimpleRuleEngine();
        var rule1 = new TestRule("SameName", RuleType.Deterministic, 50);
        var rule2 = new TestRule("SameName", RuleType.Emergent, 75);

        engine.RegisterRule(rule1);
        engine.RegisterRule(rule2);

        Assert.Single(engine.GetRulesByType(RuleType.Deterministic));
        Assert.Empty(engine.GetRulesByType(RuleType.Emergent));
    }

    [Fact]
    public void RegisterRule_MultipleRules_AllAdded()
    {
        var engine = new SimpleRuleEngine();

        engine.RegisterRule(new TestRule("A", RuleType.Deterministic, 50));
        engine.RegisterRule(new TestRule("B", RuleType.Emergent, 75));
        engine.RegisterRule(new TestRule("C", RuleType.Deterministic, 25));

        Assert.Equal(2, engine.GetRulesByType(RuleType.Deterministic).Count);
        Assert.Single(engine.GetRulesByType(RuleType.Emergent));
    }

    #endregion

    #region UnregisterRule 测试

    [Fact]
    public void UnregisterRule_RemovesRule()
    {
        var engine = new SimpleRuleEngine();
        engine.RegisterRule(new TestRule("ToRemove", RuleType.Deterministic, 50));

        engine.UnregisterRule("ToRemove");

        Assert.Empty(engine.GetRulesByType(RuleType.Deterministic));
    }

    [Fact]
    public void UnregisterRule_NonExistingName_DoesNothing()
    {
        var engine = new SimpleRuleEngine();
        engine.RegisterRule(new TestRule("Keep", RuleType.Deterministic, 50));

        engine.UnregisterRule("NonExisting");

        Assert.Single(engine.GetRulesByType(RuleType.Deterministic));
    }

    #endregion

    #region ExecuteRules 测试

    [Fact]
    public void ExecuteRules_ExecutesAllRules()
    {
        var engine = new SimpleRuleEngine();
        var rule1 = new TestRule("A", RuleType.Deterministic, 50);
        var rule2 = new TestRule("B", RuleType.Emergent, 75);
        engine.RegisterRule(rule1);
        engine.RegisterRule(rule2);

        engine.ExecuteRules(1.0f);

        Assert.Equal(1, rule1.ExecuteCount);
        Assert.Equal(1, rule2.ExecuteCount);
    }

    [Fact]
    public void ExecuteRules_MultipleTimes_IncrementsCount()
    {
        var engine = new SimpleRuleEngine();
        var rule = new TestRule("A", RuleType.Deterministic, 50);
        engine.RegisterRule(rule);

        engine.ExecuteRules(1.0f);
        engine.ExecuteRules(1.0f);
        engine.ExecuteRules(1.0f);

        Assert.Equal(3, rule.ExecuteCount);
    }

    #endregion

    #region ExecuteRulesByType 测试

    [Fact]
    public void ExecuteRulesByType_OnlyExecutesMatchingType()
    {
        var engine = new SimpleRuleEngine();
        var deterministic = new TestRule("D", RuleType.Deterministic, 50);
        var emergent = new TestRule("E", RuleType.Emergent, 75);
        engine.RegisterRule(deterministic);
        engine.RegisterRule(emergent);

        engine.ExecuteRulesByType(RuleType.Deterministic, 1.0f);

        Assert.Equal(1, deterministic.ExecuteCount);
        Assert.Equal(0, emergent.ExecuteCount);
    }

    #endregion

    #region GetRulesByPriority 测试

    [Fact]
    public void GetRulesByPriority_ReturnsMatchingRules()
    {
        var engine = new SimpleRuleEngine();
        engine.RegisterRule(new TestRule("Low", RuleType.Deterministic, 25));
        engine.RegisterRule(new TestRule("Mid", RuleType.Deterministic, 50));
        engine.RegisterRule(new TestRule("High", RuleType.Deterministic, 75));

        var rules = engine.GetRulesByPriority(30, 60);

        Assert.Single(rules);
        Assert.Equal("Mid", rules[0].Name);
    }

    [Fact]
    public void GetRulesByPriority_NoMatch_ReturnsEmpty()
    {
        var engine = new SimpleRuleEngine();
        engine.RegisterRule(new TestRule("A", RuleType.Deterministic, 50));

        var rules = engine.GetRulesByPriority(60, 100);

        Assert.Empty(rules);
    }

    #endregion

    #region 优先级排序测试

    [Fact]
    public void ExecuteRules_HigherPriorityExecutesFirst()
    {
        var engine = new SimpleRuleEngine();
        var executionOrder = new List<string>();

        engine.RegisterRule(new TestRule("Low", RuleType.Deterministic, 25));
        engine.RegisterRule(new TestRule("High", RuleType.Deterministic, 75));
        engine.RegisterRule(new TestRule("Mid", RuleType.Deterministic, 50));

        var orderedRules = engine.GetRulesByPriority(0, 100);
        Assert.Equal("High", orderedRules[0].Name);
        Assert.Equal("Mid", orderedRules[1].Name);
        Assert.Equal("Low", orderedRules[2].Name);
    }

    #endregion
}

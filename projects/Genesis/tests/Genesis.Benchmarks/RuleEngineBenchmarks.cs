using BenchmarkDotNet.Attributes;
using Genesis.Core;
using Genesis.Rules;

namespace Genesis.Benchmarks;

[MemoryDiagnoser]
public class RuleEngineBenchmarks
{
    private SimpleRuleEngine _engine = null!;
    private const int RuleCount = 100;

    [GlobalSetup]
    public void Setup()
    {
        _engine = new SimpleRuleEngine();
        for (var i = 0; i < RuleCount; i++)
        {
            _engine.RegisterRule(new BenchmarkRule($"rule_{i}", RuleType.Deterministic, i));
        }
    }

    [Benchmark(Description = "规则引擎注册规则")]
    public void RegisterRule()
    {
        var engine = new SimpleRuleEngine();
        for (var i = 0; i < RuleCount; i++)
        {
            engine.RegisterRule(new BenchmarkRule($"reg_rule_{i}", RuleType.Deterministic, i));
        }
    }

    [Benchmark(Description = "规则引擎执行全部规则")]
    public void ExecuteRules()
    {
        _engine.ExecuteRules(0.016f);
    }

    [Benchmark(Description = "规则引擎按类型执行")]
    public void ExecuteRulesByType()
    {
        _engine.ExecuteRulesByType(RuleType.Deterministic, 0.016f);
    }

    [Benchmark(Description = "规则引擎按优先级查询")]
    public IReadOnlyList<IRule> GetRulesByPriority()
    {
        return _engine.GetRulesByPriority(25, 75);
    }

    private sealed class BenchmarkRule : IRule
    {
        public string Name { get; }
        public RuleType Type { get; }
        public int Priority { get; }
        public bool IsCacheable => true;

        public BenchmarkRule(string name, RuleType type, int priority)
        {
            Name = name;
            Type = type;
            Priority = priority;
        }

        public void Execute(float deltaTime) { }
    }
}

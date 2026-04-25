using BenchmarkDotNet.Attributes;
using Genesis.Causal;

namespace Genesis.Benchmarks;

[MemoryDiagnoser]
public class CausalAnchorBenchmarks
{
    private CausalAnchor[] _anchors = null!;
    private const int AnchorCount = 1000;

    [GlobalSetup]
    public void Setup()
    {
        _anchors = new CausalAnchor[AnchorCount];
        for (var i = 0; i < AnchorCount; i++)
        {
            _anchors[i] = new CausalAnchor(
                $"anchor_{i}",
                $"锚点_{i}",
                1.0 / (i + 1),
                new Dictionary<string, double>
                {
                    ["influence"] = 0.5 + i * 0.001,
                    ["decay"] = 0.99 - i * 0.0001
                }
            );
        }
    }

    [Benchmark(Description = "因果锚点创建")]
    public CausalAnchor CreateAnchor()
    {
        return new CausalAnchor(
            "bench_anchor",
            "基准锚点",
            1.0,
            new Dictionary<string, double> { ["effect"] = 0.5 }
        );
    }

    [Benchmark(Description = "因果锚点应用增量")]
    public CausalAnchor ApplyDelta()
    {
        return _anchors[0].ApplyDelta(0.1);
    }

    [Benchmark(Description = "因果锚点批量应用增量")]
    public CausalAnchor[] BatchApplyDelta()
    {
        var results = new CausalAnchor[AnchorCount];
        for (var i = 0; i < AnchorCount; i++)
        {
            results[i] = _anchors[i].ApplyDelta(0.01 * i);
        }
        return results;
    }
}

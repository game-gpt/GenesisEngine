using BenchmarkDotNet.Attributes;
using Genesis.Collapse;

namespace Genesis.Benchmarks;

[MemoryDiagnoser]
public class CollapseBenchmarks
{
    private SimpleCollapser _collapser = null!;
    private MutableWaveFunctionState _state = null!;
    private List<IConstraint> _constraints = null!;
    private const int GridSize = 16;
    private const int CellCount = GridSize * GridSize;

    [GlobalSetup]
    public void Setup()
    {
        _collapser = new SimpleCollapser();
        _state = new MutableWaveFunctionState(CellCount, 42);
        _constraints = new List<IConstraint>();
    }

    [IterationSetup(Target = nameof(Collapse))]
    public void SetupForCollapse()
    {
        _state = new MutableWaveFunctionState(CellCount, 42);
    }

    [Benchmark(Description = "波函数坍缩")]
    public bool Collapse()
    {
        return _collapser.Collapse(_state, _constraints);
    }

    [Benchmark(Description = "波函数熵计算")]
    public double CalculateEntropy()
    {
        return _state.CalculateEntropy(0);
    }

    [Benchmark(Description = "波函数最小熵查找")]
    public int FindMinEntropyCell()
    {
        return _state.FindMinEntropyCell();
    }

    [Benchmark(Description = "波函数状态创建")]
    public MutableWaveFunctionState CreateState()
    {
        return new MutableWaveFunctionState(CellCount, 42);
    }
}

using BenchmarkDotNet.Attributes;
using Genesis.Attention;
using Genesis.Core;
using Genesis.Spacetime;

namespace Genesis.Benchmarks;

[MemoryDiagnoser]
public class AttentionBenchmarks
{
    private SimpleAttentionManager _manager = null!;
    private Position _playerPosition;
    private Position _viewDirection;
    private const int NodeCount = 100;

    [GlobalSetup]
    public void Setup()
    {
        var tree = new SpacetimeTree();
        var model = new SimpleAttentionModel();

        for (var i = 0; i < NodeCount; i++)
        {
            var spatialHash = (ulong)(i * 7919 + 104729);
            var bounds = new Bounds(i * 10, 0, 0, i * 10 + 10, 10, 10);
            var node = new HChunkNode(spatialHash, NodeLevel.L0, bounds, 42);
            tree.InsertNode(node);
        }

        _manager = new SimpleAttentionManager(tree, model);
        _playerPosition = new Position(500, 5, 5);
        _viewDirection = new Position(1, 0, 0);
    }

    [Benchmark(Description = "注意力计算")]
    public double CalculateAttention()
    {
        return _manager.CalculateAttention(new Position(505, 5, 5), _playerPosition, _viewDirection);
    }

    [Benchmark(Description = "注意力更新")]
    public void UpdateAttention()
    {
        _manager.UpdateAttention(_playerPosition, _viewDirection);
    }

    [Benchmark(Description = "获取高注意力节点")]
    public IReadOnlyList<ISpacetimeNode> GetHighAttentionNodes()
    {
        return _manager.GetHighAttentionNodes();
    }
}

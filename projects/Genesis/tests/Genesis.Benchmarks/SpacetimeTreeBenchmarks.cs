using BenchmarkDotNet.Attributes;
using Genesis.Core;
using Genesis.Spacetime;

namespace Genesis.Benchmarks;

[MemoryDiagnoser]
public class SpacetimeTreeBenchmarks
{
    private SpacetimeTree _tree = null!;
    private HChunkNode[] _nodes = null!;
    private const int NodeCount = 1000;

    [GlobalSetup]
    public void Setup()
    {
        _tree = new SpacetimeTree();
        _nodes = new HChunkNode[NodeCount];

        for (var i = 0; i < NodeCount; i++)
        {
            var spatialHash = (ulong)(i * 7919 + 104729);
            var bounds = new Bounds(i * 10, 0, 0, i * 10 + 10, 10, 10);
            _nodes[i] = new HChunkNode(spatialHash, NodeLevel.L0, bounds, 42);
        }

        for (var i = 0; i < NodeCount; i++)
        {
            _tree.InsertNode(_nodes[i]);
        }
    }

    [Benchmark(Description = "时空树插入节点")]
    public void InsertNode()
    {
        var tree = new SpacetimeTree();
        for (var i = 0; i < NodeCount; i++)
        {
            tree.InsertNode(_nodes[i]);
        }
    }

    [Benchmark(Description = "时空树查找节点")]
    public void FindNode()
    {
        for (var i = 0; i < NodeCount; i++)
        {
            _tree.FindNode(new Position(i * 10 + 5, 5, 5), NodeLevel.L0);
        }
    }

    [Benchmark(Description = "时空树更新历史哈希")]
    public void UpdateHistoryHash()
    {
        for (var i = 0; i < NodeCount; i++)
        {
            _tree.UpdateHistoryHash(_nodes[i].SpatialHash);
        }
    }

    [IterationSetup(Target = nameof(RemoveNode))]
    public void SetupForRemove()
    {
        _tree = new SpacetimeTree();
        for (var i = 0; i < NodeCount; i++)
        {
            _tree.InsertNode(_nodes[i]);
        }
    }

    [Benchmark(Description = "时空树删除节点")]
    public void RemoveNode()
    {
        for (var i = 0; i < NodeCount; i++)
        {
            _tree.RemoveNode(_nodes[i].SpatialHash);
        }
    }
}

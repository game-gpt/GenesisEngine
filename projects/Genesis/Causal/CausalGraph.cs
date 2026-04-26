using Genesis.Core;

namespace Genesis.Causal;

public sealed class CausalGraph
{
    #region 字段

    private readonly Dictionary<string, CausalNode> _nodes = new();
    private readonly Dictionary<string, List<CausalEdge>> _outgoingEdges = new();
    private readonly Dictionary<string, List<CausalEdge>> _incomingEdges = new();
    private readonly CausalWeightSystem _weightSystem;

    #endregion

    #region 属性

    public int NodeCount => _nodes.Count;
    public int EdgeCount => _outgoingEdges.Values.Sum(e => e.Count);
    public CausalWeightSystem WeightSystem => _weightSystem;

    #endregion

    #region 构造函数

    public CausalGraph(int featureDimension = 8)
    {
        _weightSystem = new CausalWeightSystem(featureDimension);
    }

    #endregion

    #region 节点操作

    public string AddNode(string name, AnchorType type, double initialWeight = 1.0, PlayerId? playerId = null)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var anchor = new MutableCausalAnchor(id, name, initialWeight, new Dictionary<string, double>());

        _nodes[id] = new CausalNode(id, name, type, initialWeight, playerId);

        if (playerId.HasValue)
        {
            _weightSystem.AddAnchor(playerId.Value, anchor);
        }

        _outgoingEdges[id] = new List<CausalEdge>();
        _incomingEdges[id] = new List<CausalEdge>();

        return id;
    }

    public bool RemoveNode(string nodeId)
    {
        if (!_nodes.ContainsKey(nodeId))
        {
            return false;
        }

        var node = _nodes[nodeId];

        if (node.PlayerId.HasValue)
        {
            _weightSystem.RemoveAnchor(node.PlayerId.Value, nodeId);
        }

        if (_outgoingEdges.TryGetValue(nodeId, out var outEdges))
        {
            foreach (var edge in outEdges)
            {
                if (_incomingEdges.TryGetValue(edge.TargetId, out var inList))
                {
                    inList.RemoveAll(e => e.SourceId == nodeId);
                }
            }
        }

        if (_incomingEdges.TryGetValue(nodeId, out var inEdges))
        {
            foreach (var edge in inEdges)
            {
                if (_outgoingEdges.TryGetValue(edge.SourceId, out var outList))
                {
                    outList.RemoveAll(e => e.TargetId == nodeId);
                }
            }
        }

        _nodes.Remove(nodeId);
        _outgoingEdges.Remove(nodeId);
        _incomingEdges.Remove(nodeId);

        return true;
    }

    public CausalNode? GetNode(string nodeId)
    {
        return _nodes.GetValueOrDefault(nodeId);
    }

    public IReadOnlyList<CausalNode> GetAllNodes()
    {
        return _nodes.Values.ToList();
    }

    public IReadOnlyList<CausalNode> GetNodesByType(AnchorType type)
    {
        return _nodes.Values.Where(n => n.Type == type).ToList();
    }

    public IReadOnlyList<CausalNode> GetNodesByPlayer(PlayerId playerId)
    {
        return _nodes.Values.Where(n => n.PlayerId.HasValue && n.PlayerId.Value.Equals(playerId)).ToList();
    }

    #endregion

    #region 边操作

    public void AddEdge(string sourceId, string targetId, double weight, string? label = null)
    {
        if (!_nodes.ContainsKey(sourceId) || !_nodes.ContainsKey(targetId))
        {
            return;
        }

        var edge = new CausalEdge(sourceId, targetId, weight, label);
        _outgoingEdges[sourceId].Add(edge);
        _incomingEdges[targetId].Add(edge);
    }

    public IReadOnlyList<CausalEdge> GetOutgoingEdges(string nodeId)
    {
        return _outgoingEdges.GetValueOrDefault(nodeId, new List<CausalEdge>());
    }

    public IReadOnlyList<CausalEdge> GetIncomingEdges(string nodeId)
    {
        return _incomingEdges.GetValueOrDefault(nodeId, new List<CausalEdge>());
    }

    #endregion

    #region 因果链追踪

    public IReadOnlyList<CausalChain> TraceForward(string startNodeId, int maxDepth = 10)
    {
        var chains = new List<CausalChain>();
        var currentPath = new List<CausalEdge>();
        TraceForwardRecursive(startNodeId, maxDepth, currentPath, chains);
        return chains;
    }

    public IReadOnlyList<CausalChain> TraceBackward(string endNodeId, int maxDepth = 10)
    {
        var chains = new List<CausalChain>();
        var currentPath = new List<CausalEdge>();
        TraceBackwardRecursive(endNodeId, maxDepth, currentPath, chains);
        return chains;
    }

    public IReadOnlyList<CausalNode> GetRootCauses(string nodeId, int maxDepth = 10)
    {
        var visited = new HashSet<string>();
        var roots = new List<CausalNode>();
        FindRootCausesRecursive(nodeId, maxDepth, visited, roots);
        return roots;
    }

    public IReadOnlyList<CausalNode> GetEffects(string nodeId, int maxDepth = 10)
    {
        var visited = new HashSet<string>();
        var effects = new List<CausalNode>();
        FindEffectsRecursive(nodeId, maxDepth, visited, effects);
        return effects;
    }

    #endregion

    #region 因果推理

    public double ComputeCausalStrength(string sourceId, string targetId)
    {
        var chains = TraceForward(sourceId);
        var strength = 0.0;

        foreach (var chain in chains)
        {
            if (chain.Nodes.LastOrDefault()?.Id == targetId)
            {
                strength += chain.AggregatedWeight;
            }
        }

        return Math.Min(strength, 1.0);
    }

    public IReadOnlyList<(CausalNode Node, double Strength)> FindStrongestCauses(string targetId, int topN = 5)
    {
        var causes = new List<(CausalNode Node, double Strength)>();
        var incoming = GetIncomingEdges(targetId);

        foreach (var edge in incoming)
        {
            var sourceNode = GetNode(edge.SourceId);
            if (sourceNode is not null)
            {
                var indirectStrength = ComputeCausalStrength(edge.SourceId, targetId);
                causes.Add((sourceNode, edge.Weight + indirectStrength));
            }
        }

        return causes
            .OrderByDescending(c => c.Strength)
            .Take(topN)
            .ToList();
    }

    #endregion

    #region 私有方法

    private void TraceForwardRecursive(string currentId, int remainingDepth, List<CausalEdge> path, List<CausalChain> chains)
    {
        if (remainingDepth <= 0)
        {
            return;
        }

        var edges = GetOutgoingEdges(currentId);
        if (edges.Count == 0 && path.Count > 0)
        {
            chains.Add(BuildChain(path));
            return;
        }

        foreach (var edge in edges)
        {
            path.Add(edge);
            TraceForwardRecursive(edge.TargetId, remainingDepth - 1, path, chains);
            path.RemoveAt(path.Count - 1);
        }
    }

    private void TraceBackwardRecursive(string currentId, int remainingDepth, List<CausalEdge> path, List<CausalChain> chains)
    {
        if (remainingDepth <= 0)
        {
            return;
        }

        var edges = GetIncomingEdges(currentId);
        if (edges.Count == 0 && path.Count > 0)
        {
            chains.Add(BuildChain(path));
            return;
        }

        foreach (var edge in edges)
        {
            path.Insert(0, edge);
            TraceBackwardRecursive(edge.SourceId, remainingDepth - 1, path, chains);
            path.RemoveAt(0);
        }
    }

    private void FindRootCausesRecursive(string currentId, int remainingDepth, HashSet<string> visited, List<CausalNode> roots)
    {
        if (!visited.Add(currentId) || remainingDepth <= 0)
        {
            return;
        }

        var incoming = GetIncomingEdges(currentId);
        if (incoming.Count == 0)
        {
            var node = GetNode(currentId);
            if (node is not null)
            {
                roots.Add(node);
            }
            return;
        }

        foreach (var edge in incoming)
        {
            FindRootCausesRecursive(edge.SourceId, remainingDepth - 1, visited, roots);
        }
    }

    private void FindEffectsRecursive(string currentId, int remainingDepth, HashSet<string> visited, List<CausalNode> effects)
    {
        if (!visited.Add(currentId) || remainingDepth <= 0)
        {
            return;
        }

        var outgoing = GetOutgoingEdges(currentId);
        if (outgoing.Count == 0)
        {
            var node = GetNode(currentId);
            if (node is not null)
            {
                effects.Add(node);
            }
            return;
        }

        foreach (var edge in outgoing)
        {
            FindEffectsRecursive(edge.TargetId, remainingDepth - 1, visited, effects);
        }
    }

    private CausalChain BuildChain(List<CausalEdge> edges)
    {
        var nodes = new List<CausalNode>();
        var totalWeight = 0.0;

        foreach (var edge in edges)
        {
            var node = GetNode(edge.SourceId);
            if (node is not null)
            {
                nodes.Add(node);
            }
            totalWeight += edge.Weight;
        }

        var lastEdge = edges[^1];
        var lastNode = GetNode(lastEdge.TargetId);
        if (lastNode is not null)
        {
            nodes.Add(lastNode);
        }

        var aggregatedWeight = edges.Count > 0 ? totalWeight / edges.Count : 0;
        return new CausalChain(nodes, edges, aggregatedWeight);
    }

    #endregion

    #region 内部类型

    public sealed class CausalNode
    {
        public string Id { get; }
        public string Name { get; }
        public AnchorType Type { get; }
        public double Weight { get; set; }
        public PlayerId? PlayerId { get; }

        public CausalNode(string id, string name, AnchorType type, double weight, PlayerId? playerId)
        {
            Id = id;
            Name = name;
            Type = type;
            Weight = weight;
            PlayerId = playerId;
        }
    }

    public sealed class CausalEdge
    {
        public string SourceId { get; }
        public string TargetId { get; }
        public double Weight { get; }
        public string? Label { get; }

        public CausalEdge(string sourceId, string targetId, double weight, string? label)
        {
            SourceId = sourceId;
            TargetId = targetId;
            Weight = weight;
            Label = label;
        }
    }

    public sealed class CausalChain
    {
        public IReadOnlyList<CausalNode> Nodes { get; }
        public IReadOnlyList<CausalEdge> Edges { get; }
        public double AggregatedWeight { get; }

        public CausalChain(IReadOnlyList<CausalNode> nodes, IReadOnlyList<CausalEdge> edges, double aggregatedWeight)
        {
            Nodes = nodes;
            Edges = edges;
            AggregatedWeight = aggregatedWeight;
        }
    }

    private sealed class MutableCausalAnchor : ICausalAnchor
    {
        public string Id { get; }
        public string Name { get; }
        public double Weight { get; private set; }
        public IReadOnlyDictionary<string, double> Effects { get; }

        private readonly Dictionary<string, double> _effects;

        public MutableCausalAnchor(string id, string name, double weight, Dictionary<string, double> effects)
        {
            Id = id;
            Name = name;
            Weight = weight;
            _effects = effects;
            Effects = _effects;
        }

        public void ApplyWeight(double delta)
        {
            Weight += delta;
        }
    }

    #endregion
}

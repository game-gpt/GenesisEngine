namespace Genesis.HashLife;

public sealed class HashLifeEvolver : IHashLifeEvolver
{
    #region 字段

    private readonly IHashLifeCache _cache;
    private readonly Dictionary<ulong, QuadNode> _nodes = new();
    private readonly Dictionary<ulong, ulong> _evolvedCache = new();
    private readonly Dictionary<int, ulong> _emptyNodes = new();

    #endregion

    #region 构造函数

    public HashLifeEvolver(IHashLifeCache cache)
    {
        _cache = cache;
        InitializeEmptyNodes();
    }

    #endregion

    #region IHashLifeEvolver 实现

    public ulong Evolve(ulong nodeHash, int steps)
    {
        if (steps == 0)
        {
            return nodeHash;
        }

        if (!_nodes.TryGetValue(nodeHash, out var node))
        {
            return nodeHash;
        }

        if (steps == 1)
        {
            return EvolveOneStep(nodeHash);
        }

        var level = node.Level;
        var stepSize = 1 << (level - 1);

        if (steps >= stepSize)
        {
            return EvolveFullStep(nodeHash);
        }

        return EvolvePartialStep(nodeHash, steps);
    }

    public ulong Evolve(ulong nodeHash, float deltaTime)
    {
        if (deltaTime <= 0)
        {
            return nodeHash;
        }

        if (_cache.TryGet(nodeHash, deltaTime, out var resultHash))
        {
            return resultHash;
        }

        var steps = Math.Max(1, (int)(deltaTime * 60));
        var evolved = Evolve(nodeHash, steps);

        _cache.Set(nodeHash, deltaTime, evolved);
        return evolved;
    }

    public int GetNodeLevel(ulong nodeHash)
    {
        if (_nodes.TryGetValue(nodeHash, out var node))
        {
            return node.Level;
        }

        return 0;
    }

    public ulong GetEmptyNode(int level)
    {
        return _emptyNodes.TryGetValue(level, out var hash) ? hash : 0;
    }

    #endregion

    #region 公开方法

    public ulong RegisterLeafNode(ulong cellState)
    {
        var hash = ComputeHash(cellState, 0, 0, 0);
        _nodes[hash] = new QuadNode(0, cellState, 0, 0, 0);
        return hash;
    }

    public ulong RegisterInternalNode(int level, ulong nw, ulong ne, ulong sw, ulong se)
    {
        var hash = ComputeHash(nw, ne, sw, se);
        _nodes[hash] = new QuadNode(level, nw, ne, sw, se);
        return hash;
    }

    public QuadNode? GetNode(ulong hash)
    {
        return _nodes.TryGetValue(hash, out var node) ? node : null;
    }

    #endregion

    #region 私有方法 - 演化核心

    private ulong EvolveOneStep(ulong nodeHash)
    {
        if (_evolvedCache.TryGetValue(nodeHash, out var cached))
        {
            return cached;
        }

        if (!_nodes.TryGetValue(nodeHash, out var node))
        {
            return nodeHash;
        }

        if (node.Level == 0)
        {
            return nodeHash;
        }

        if (node.Level == 1)
        {
            return EvolveLeaf(node);
        }

        var result = EvolveInternalOneStep(node);
        _evolvedCache[nodeHash] = result;
        return result;
    }

    private ulong EvolveLeaf(QuadNode node)
    {
        return node.NW;
    }

    private ulong EvolveInternalOneStep(QuadNode node)
    {
        if (!_nodes.TryGetValue(node.NW, out var nw) ||
            !_nodes.TryGetValue(node.NE, out var ne) ||
            !_nodes.TryGetValue(node.SW, out var sw) ||
            !_nodes.TryGetValue(node.SE, out var se))
        {
            return node.Hash;
        }

        var nwn = MakeNode(nw.NW, nw.NE, nw.SW, nw.SE);
        var nne = MakeNode(nw.NE, ne.NW, nw.SE, ne.SW);
        var nww = MakeNode(nw.SW, nw.SE, sw.NW, sw.NE);
        var ncc = MakeNode(nw.SE, ne.SW, sw.NE, se.NW);
        var nee = MakeNode(ne.SW, ne.SE, se.NW, se.NE);
        var sww = MakeNode(sw.NW, sw.NE, sw.SW, sw.SE);
        var scc = MakeNode(sw.NE, se.NW, sw.SE, se.SW);
        var see = MakeNode(se.NW, se.NE, se.SW, se.SE);

        var innerNw = EvolveOneStep(MakeNode(nwn, nne, nww, ncc));
        var innerNe = EvolveOneStep(MakeNode(nne, nee, ncc, nee));
        var innerSw = EvolveOneStep(MakeNode(nww, ncc, sww, scc));
        var innerSe = EvolveOneStep(MakeNode(ncc, see, scc, see));

        return MakeNode(innerNw, innerNe, innerSw, innerSe);
    }

    private ulong EvolveFullStep(ulong nodeHash)
    {
        if (!_nodes.TryGetValue(nodeHash, out var node))
        {
            return nodeHash;
        }

        var center = ComputeCenter(node);
        return EvolveOneStep(center);
    }

    private ulong EvolvePartialStep(ulong nodeHash, int steps)
    {
        if (!_nodes.TryGetValue(nodeHash, out var node))
        {
            return nodeHash;
        }

        var current = nodeHash;
        for (var i = 0; i < steps; i++)
        {
            current = EvolveOneStep(current);
            if (current == 0)
            {
                break;
            }
        }

        return current;
    }

    private ulong ComputeCenter(QuadNode node)
    {
        if (!_nodes.TryGetValue(node.NW, out var nw) ||
            !_nodes.TryGetValue(node.NE, out var ne) ||
            !_nodes.TryGetValue(node.SW, out var sw) ||
            !_nodes.TryGetValue(node.SE, out var se))
        {
            return node.Hash;
        }

        return MakeNode(nw.SE, ne.SW, sw.NE, se.NW);
    }

    private ulong MakeNode(ulong nw, ulong ne, ulong sw, ulong se)
    {
        var level = 0;
        if (_nodes.TryGetValue(nw, out var nwNode))
        {
            level = nwNode.Level + 1;
        }

        var hash = ComputeHash(nw, ne, sw, se);
        if (!_nodes.ContainsKey(hash))
        {
            _nodes[hash] = new QuadNode(level, nw, ne, sw, se);
        }

        return hash;
    }

    #endregion

    #region 私有方法 - 哈希与初始化

    private void InitializeEmptyNodes()
    {
        var emptyLeaf = RegisterLeafNode(0);
        _emptyNodes[0] = emptyLeaf;

        for (var level = 1; level <= 32; level++)
        {
            var prev = _emptyNodes[level - 1];
            var emptyNode = RegisterInternalNode(level, prev, prev, prev, prev);
            _emptyNodes[level] = emptyNode;
        }
    }

    private static ulong ComputeHash(ulong nw, ulong ne, ulong sw, ulong se)
    {
        unchecked
        {
            ulong hash = 14695981039346656037;
            hash ^= nw;
            hash *= 1099511628211;
            hash ^= ne;
            hash *= 1099511628211;
            hash ^= sw;
            hash *= 1099511628211;
            hash ^= se;
            hash *= 1099511628211;
            return hash;
        }
    }

    #endregion

    #region 内部类型

    public sealed class QuadNode
    {
        public int Level { get; }
        public ulong NW { get; }
        public ulong NE { get; }
        public ulong SW { get; }
        public ulong SE { get; }
        public ulong Hash { get; }

        public QuadNode(int level, ulong nw, ulong ne, ulong sw, ulong se)
        {
            Level = level;
            NW = nw;
            NE = ne;
            SW = sw;
            SE = se;
            Hash = ComputeHash(nw, ne, sw, se);
        }
    }

    #endregion
}

using Genesis.Attention;
using Genesis.Causal;
using Genesis.Core;
using Genesis.Rules;
using Genesis.Spacetime;
using Gnosis.Core;
using Gnosis.ECS.World;
using Gnosis.Runtime.VM;

namespace Genesis.Runtime;

public sealed class EmergentNativeBridge
{
    #region 常量

    private const int BaseId = 10000;
    private const ulong DefaultWorldSeed = 42;

    #endregion

    #region 字段

    private readonly NativeFunctionRegistry _registry;
    private SpacetimeTree? _spacetimeTree;
    private CausalGraph? _causalGraph;
    private SimpleAttentionManager? _attentionManager;
    private SimpleRuleEngine? _ruleEngine;
    private Gnosis.ECS.World.World? _world;
    private ulong _worldSeed;

    #endregion

    #region 属性

    public SpacetimeTree? SpacetimeTree
    {
        get => _spacetimeTree;
        set => _spacetimeTree = value;
    }

    public CausalGraph? CausalGraph
    {
        get => _causalGraph;
        set => _causalGraph = value;
    }

    public SimpleAttentionManager? AttentionManager
    {
        get => _attentionManager;
        set => _attentionManager = value;
    }

    public SimpleRuleEngine? RuleEngine
    {
        get => _ruleEngine;
        set => _ruleEngine = value;
    }

    public ulong WorldSeed
    {
        get => _worldSeed;
        set => _worldSeed = value;
    }

    #endregion

    #region 构造函数

    public EmergentNativeBridge(NativeFunctionRegistry registry)
    {
        _registry = registry;
        _worldSeed = DefaultWorldSeed;
    }

    #endregion

    #region 注册

    public void RegisterAll()
    {
        RegisterSpacetimeFunctions();
        RegisterCausalFunctions();
        RegisterAttentionFunctions();
        RegisterRuleFunctions();
    }

    #endregion

    #region Spacetime 原生函数

    private void RegisterSpacetimeFunctions()
    {
        _registry.Register(new SimpleNativeFunction(BaseId + 1, "spacetime_insert", 3, (vm, args) =>
        {
            if (_spacetimeTree is null)
            {
                return GGValue.FromBool(false);
            }

            var x = (float)args[0].FloatValue;
            var y = (float)args[1].FloatValue;
            var z = (float)args[2].FloatValue;

            var position = new Position(x, y, z);
            var hash = SpatialHasher.ComputeSpatialHash(position, NodeLevel.L0, _worldSeed);
            var bounds = new Bounds(x - 0.5, y - 0.5, z - 0.5, x + 0.5, y + 0.5, z + 0.5);
            var node = new HChunkNode(hash, NodeLevel.L0, bounds, _worldSeed);
            _spacetimeTree.InsertNode(node);

            return GGValue.FromBool(true);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 2, "spacetime_find", 3, (vm, args) =>
        {
            if (_spacetimeTree is null)
            {
                return GGValue.Null;
            }

            var x = (float)args[0].FloatValue;
            var y = (float)args[1].FloatValue;
            var z = (float)args[2].FloatValue;

            var position = new Position(x, y, z);
            var node = _spacetimeTree.FindNode(position, NodeLevel.L0);

            if (node is null)
            {
                return GGValue.Null;
            }

            return GGValue.FromNativeObject(new SpacetimeNodeHandle(node));
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 3, "spacetime_remove", 3, (vm, args) =>
        {
            if (_spacetimeTree is null)
            {
                return GGValue.FromBool(false);
            }

            var x = (float)args[0].FloatValue;
            var y = (float)args[1].FloatValue;
            var z = (float)args[2].FloatValue;

            var position = new Position(x, y, z);
            var hash = SpatialHasher.ComputeSpatialHash(position, NodeLevel.L0, _worldSeed);

            _spacetimeTree.RemoveNode(hash);

            return GGValue.FromBool(true);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 4, "spacetime_get_history_hash", 3, (vm, args) =>
        {
            if (_spacetimeTree is null)
            {
                return GGValue.FromInt(0);
            }

            var x = (float)args[0].FloatValue;
            var y = (float)args[1].FloatValue;
            var z = (float)args[2].FloatValue;

            var position = new Position(x, y, z);
            var node = _spacetimeTree.FindNode(position, NodeLevel.L0);

            if (node is null)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt((long)node.HistoryHash);
        }));
    }

    #endregion

    #region Causal 原生函数

    private void RegisterCausalFunctions()
    {
        _registry.Register(new SimpleNativeFunction(BaseId + 100, "causal_add_node", 3, (vm, args) =>
        {
            if (_causalGraph is null)
            {
                return GGValue.Null;
            }

            var name = args[0].StringValue?.Value ?? "unnamed";
            var typeInt = (int)args[1].IntValue;
            var type = (AnchorType)typeInt;
            var weight = args[2].FloatValue;

            var nodeId = _causalGraph.AddNode(name, type, weight);
            return GGValue.FromString(new GGString(nodeId));
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 101, "causal_add_edge", 3, (vm, args) =>
        {
            if (_causalGraph is null)
            {
                return GGValue.FromBool(false);
            }

            var fromId = args[0].StringValue?.Value ?? "";
            var toId = args[1].StringValue?.Value ?? "";
            var weight = args[2].FloatValue;

            _causalGraph.AddEdge(fromId, toId, weight);
            return GGValue.FromBool(true);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 102, "causal_trace_forward", 1, (vm, args) =>
        {
            if (_causalGraph is null)
            {
                return GGValue.FromArray(new GGArray(0));
            }

            var nodeId = args[0].StringValue?.Value ?? "";
            var chains = _causalGraph.TraceForward(nodeId);
            var values = chains
                .SelectMany(c => c.Nodes.Select(n => GGValue.FromString(new GGString(n.Id))))
                .ToList();

            var arr = new GGArray(values.Count);
            foreach (var v in values)
            {
                arr.Add(v);
            }

            return GGValue.FromArray(arr);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 103, "causal_trace_backward", 1, (vm, args) =>
        {
            if (_causalGraph is null)
            {
                return GGValue.FromArray(new GGArray(0));
            }

            var nodeId = args[0].StringValue?.Value ?? "";
            var chains = _causalGraph.TraceBackward(nodeId);
            var values = chains
                .SelectMany(c => c.Nodes.Select(n => GGValue.FromString(new GGString(n.Id))))
                .ToList();

            var arr = new GGArray(values.Count);
            foreach (var v in values)
            {
                arr.Add(v);
            }

            return GGValue.FromArray(arr);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 104, "causal_get_root_causes", 1, (vm, args) =>
        {
            if (_causalGraph is null)
            {
                return GGValue.Null;
            }

            var nodeId = args[0].StringValue?.Value ?? "";
            var roots = _causalGraph.GetRootCauses(nodeId);

            if (roots.Count == 0)
            {
                return GGValue.Null;
            }

            return GGValue.FromString(new GGString(roots[0].Id));
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 105, "causal_node_count", 0, (vm, args) =>
        {
            if (_causalGraph is null)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt(_causalGraph.NodeCount);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 106, "causal_edge_count", 0, (vm, args) =>
        {
            if (_causalGraph is null)
            {
                return GGValue.FromInt(0);
            }

            return GGValue.FromInt(_causalGraph.EdgeCount);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 107, "causal_get_weight", 1, (vm, args) =>
        {
            if (_causalGraph is null)
            {
                return GGValue.FromFloat(0.0);
            }

            var nodeId = args[0].StringValue?.Value ?? "";
            var node = _causalGraph.GetNode(nodeId);

            if (node is null)
            {
                return GGValue.FromFloat(0.0);
            }

            return GGValue.FromFloat(node.Weight);
        }));
    }

    #endregion

    #region Attention 原生函数

    private void RegisterAttentionFunctions()
    {
        _registry.Register(new SimpleNativeFunction(BaseId + 200, "attention_calculate", 6, (vm, args) =>
        {
            if (_attentionManager is null)
            {
                return GGValue.FromFloat(0.0);
            }

            var posX = (float)args[0].FloatValue;
            var posY = (float)args[1].FloatValue;
            var posZ = (float)args[2].FloatValue;
            var playerX = (float)args[3].FloatValue;
            var playerY = (float)args[4].FloatValue;
            var playerZ = (float)args[5].FloatValue;

            var position = new Position(posX, posY, posZ);
            var playerPosition = new Position(playerX, playerY, playerZ);
            var playerView = new Position(0, 0, 1);

            var attention = _attentionManager.CalculateAttention(position, playerPosition, playerView);
            return GGValue.FromFloat(attention);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 201, "attention_add_interest_point", 4, (vm, args) =>
        {
            if (_attentionManager is null)
            {
                return GGValue.FromBool(false);
            }

            var x = (float)args[0].FloatValue;
            var y = (float)args[1].FloatValue;
            var z = (float)args[2].FloatValue;
            var radius = (float)args[3].FloatValue;

            var id = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var point = new InterestPoint(id, new Position(x, y, z), radius, 1.0, "script");
            _attentionManager.AddInterestPoint(point);

            return GGValue.FromBool(true);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 202, "attention_high_attention_count", 0, (vm, args) =>
        {
            if (_attentionManager is null)
            {
                return GGValue.FromInt(0);
            }

            var nodes = _attentionManager.GetHighAttentionNodes();
            return GGValue.FromInt(nodes.Count);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 203, "attention_set_player_position", 3, (vm, args) =>
        {
            if (_attentionManager is null)
            {
                return GGValue.FromBool(false);
            }

            var x = (float)args[0].FloatValue;
            var y = (float)args[1].FloatValue;
            var z = (float)args[2].FloatValue;

            _attentionManager.UpdateAttention(new Position(x, y, z), new Position(0, 0, 1));
            return GGValue.FromBool(true);
        }));
    }

    #endregion

    #region Rule 原生函数

    private void RegisterRuleFunctions()
    {
        _registry.Register(new SimpleNativeFunction(BaseId + 300, "rule_register", 3, (vm, args) =>
        {
            if (_ruleEngine is null)
            {
                return GGValue.FromBool(false);
            }

            var name = args[0].StringValue?.Value ?? "";
            var priority = (int)args[1].IntValue;
            var typeInt = (int)args[2].IntValue;

            var rule = new NativeDeterministicRule(name, priority, (RuleType)typeInt);
            _ruleEngine.RegisterRule(rule);

            return GGValue.FromBool(true);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 301, "rule_unregister", 1, (vm, args) =>
        {
            if (_ruleEngine is null)
            {
                return GGValue.FromBool(false);
            }

            var name = args[0].StringValue?.Value ?? "";
            _ruleEngine.UnregisterRule(name);

            return GGValue.FromBool(true);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 302, "rule_execute_all", 1, (vm, args) =>
        {
            if (_ruleEngine is null)
            {
                return GGValue.FromBool(false);
            }

            var delta = (float)args[0].FloatValue;
            _ruleEngine.ExecuteRules(delta);

            return GGValue.FromBool(true);
        }));

        _registry.Register(new SimpleNativeFunction(BaseId + 303, "rule_execute_by_type", 2, (vm, args) =>
        {
            if (_ruleEngine is null)
            {
                return GGValue.FromBool(false);
            }

            var typeInt = (int)args[0].IntValue;
            var delta = (float)args[1].FloatValue;

            _ruleEngine.ExecuteRulesByType((RuleType)typeInt, delta);
            return GGValue.FromBool(true);
        }));
    }

    #endregion

    #region 内部类型

    private sealed class SimpleNativeFunction : INativeFunction
    {
        private readonly Func<IVMState, GGValue[], GGValue> _implementation;

        public int Id { get; }
        public string Name { get; }
        public int ParameterCount { get; }

        public SimpleNativeFunction(int id, string name, int parameterCount, Func<IVMState, GGValue[], GGValue> implementation)
        {
            Id = id;
            Name = name;
            ParameterCount = parameterCount;
            _implementation = implementation;
        }

        public GGValue Execute(IVMState vm, GGValue[] args)
        {
            return _implementation(vm, args);
        }
    }

    private sealed class SpacetimeNodeHandle
    {
        public ISpacetimeNode Node { get; }

        public SpacetimeNodeHandle(ISpacetimeNode node)
        {
            Node = node;
        }
    }

    private sealed class NativeDeterministicRule : IRule
    {
        public string Name { get; }
        public int Priority { get; }
        public RuleType Type { get; }
        public bool IsCacheable => false;

        public NativeDeterministicRule(string name, int priority, RuleType type)
        {
            Name = name;
            Priority = priority;
            Type = type;
        }

        public void Execute(float deltaTime)
        {
        }
    }

    #endregion
}

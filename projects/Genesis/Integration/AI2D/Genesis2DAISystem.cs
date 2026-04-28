using Gnosis.AI.Behavior;
using Gnosis.AI.Blackboard;
using Gnosis.AI.State;
using Gnosis.ECS.System;
using Gnosis.ECS.World;

namespace Genesis.Integration.AI2D;

[Obsolete("请使用 Genesis.GameSystems.AIGameSystem 替代。Integration 层将在未来版本移除。")]
public sealed class Genesis2DAISystem : ISystem, IWorldSystem
{
    #region 字段

    private Gnosis.ECS.World.World? _world;
    private IAISystem? _aiSystem;
    private readonly Dictionary<string, IBehaviorTree> _behaviorTrees = new();
    private readonly Dictionary<string, IAIController> _controllers = new();
    private bool _initialized;

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.Update;

    public IAISystem? System => _aiSystem;

    public bool IsInitialized => _initialized;

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _aiSystem = new AISystem();
        _initialized = true;
        Console.WriteLine("[Genesis] 2D AI 系统初始化完成");
    }

    public void Shutdown()
    {
        foreach (var controller in _controllers.Values)
        {
            _aiSystem?.DestroyController(controller);
        }

        _controllers.Clear();
        _behaviorTrees.Clear();
        _initialized = false;
    }

    #endregion

    #region IWorldSystem 实现

    public void SetWorld(Gnosis.ECS.World.World world)
    {
        _world = world;
    }

    #endregion

    #region ISystem.Update

    public void Update(float delta)
    {
        if (!_initialized || _aiSystem is null)
        {
            return;
        }

        _aiSystem.Update(delta);
    }

    #endregion

    #region 公开方法

    public IBehaviorTree CreateBehaviorTree(string name, IBlackboard? blackboard = null)
    {
        if (_aiSystem is null)
        {
            throw new InvalidOperationException("2D AI 系统未初始化");
        }

        var tree = _aiSystem.CreateBehaviorTree(name);

        if (blackboard is not null && tree is BehaviorTree bt)
        {
            var root = new BTNodes.Sequence($"{name}_root");
            bt.SetRoot(root);
        }

        _behaviorTrees[name] = tree;
        return tree;
    }

    public IBehaviorTree? GetBehaviorTree(string name)
    {
        return _behaviorTrees.GetValueOrDefault(name);
    }

    public void StartBehaviorTree(string name)
    {
        if (_behaviorTrees.TryGetValue(name, out var tree))
        {
            tree.Start();
        }
    }

    public void StopBehaviorTree(string name)
    {
        if (_behaviorTrees.TryGetValue(name, out var tree))
        {
            tree.Stop();
        }
    }

    public IAIController CreateController(string name)
    {
        if (_aiSystem is null)
        {
            throw new InvalidOperationException("2D AI 系统未初始化");
        }

        var controller = _aiSystem.CreateController();
        _controllers[name] = controller;
        return controller;
    }

    public void DestroyController(string name)
    {
        if (_aiSystem is null)
        {
            return;
        }

        if (_controllers.TryGetValue(name, out var controller))
        {
            _aiSystem.DestroyController(controller);
            _controllers.Remove(name);
        }
    }

    #endregion
}

using Gnosis.AI.Behavior;
using Gnosis.AI.Blackboard;
using Gnosis.AI.State;

namespace Genesis.Integration;

public sealed class AIIntegration : IDisposable
{
    #region 字段

    private IAISystem? _aiSystem;
    private readonly Dictionary<string, IBehaviorTree> _behaviorTrees = new();
    private readonly Dictionary<string, IAIController> _controllers = new();
    private bool _disposed;

    #endregion

    #region 属性

    public IAISystem? System => _aiSystem;

    public bool IsInitialized => _aiSystem is not null;

    #endregion

    #region 初始化

    public void Initialize(IAISystem? aiSystem = null)
    {
        _aiSystem = aiSystem ?? new Gnosis.AI.State.AISystem();

        Console.WriteLine("[Genesis] AI 系统初始化完成");
    }

    #endregion

    #region 行为树管理

    public IBehaviorTree CreateBehaviorTree(string name, IBlackboard? blackboard = null)
    {
        if (_aiSystem is null)
        {
            throw new InvalidOperationException("AI 系统未初始化，请先调用 Initialize()");
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

    #endregion

    #region AI 控制器

    public IAIController CreateController(string name)
    {
        if (_aiSystem is null)
        {
            throw new InvalidOperationException("AI 系统未初始化，请先调用 Initialize()");
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

    #region 更新

    public void Update(float delta)
    {
        _aiSystem?.Update(delta);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var controller in _controllers.Values)
        {
            _aiSystem?.DestroyController(controller);
        }

        _controllers.Clear();
        _behaviorTrees.Clear();

        _disposed = true;
    }

    #endregion
}

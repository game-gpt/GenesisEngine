using Genesis.Core;
using Genesis.HAL;
using Gnosis.AI.Behavior;
using Gnosis.AI.Blackboard;
using Gnosis.AI.State;
using ISystem = Gnosis.ECS.System.ISystem;
using IWorldSystem = Gnosis.ECS.System.IWorldSystem;
using SystemPhase = Gnosis.ECS.System.SystemPhase;
using GnosisWorld = Gnosis.ECS.World.World;

namespace Genesis.GameSystems;

/// <summary>
/// AI 游戏系统
/// 从 Genesis2DAISystem 重命名而来（AI 与维度无关，不需要 2D/3D 分裂）
/// 通过 HAL 接口访问实体世界
/// </summary>
public sealed class AIGameSystem : ISystem, IWorldSystem
{
    #region 字段

    private GnosisWorld? _world;
    private IEntityWorld? _entityWorld;
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

    #region 构造函数

    public AIGameSystem()
    {
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _aiSystem = new AISystem();
        _initialized = true;
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

    public void SetWorld(GnosisWorld world)
    {
        _world = world;
    }

    /// <summary>
    /// 设置 HAL 实体世界
    /// </summary>
    public void SetEntityWorld(IEntityWorld entityWorld)
    {
        _entityWorld = entityWorld;
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
            throw new InvalidOperationException("AI 系统未初始化");
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
            throw new InvalidOperationException("AI 系统未初始化");
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

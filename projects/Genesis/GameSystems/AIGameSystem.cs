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
/// </summary>
public sealed class AIGameSystem : ISystem, IWorldSystem
{
    #region 字段

    private GnosisWorld? _world;
    private IAISystem? _aiSystem;
    private readonly Dictionary<string, IBehaviorTree> _behaviorTrees = new();
    private readonly Dictionary<string, IAIController> _controllers = new();
    private bool _initialized;

    #endregion

    #region 属性

    /// <summary>
    /// 系统执行阶段
    /// </summary>
    public SystemPhase Phase => SystemPhase.Update;

    /// <summary>
    /// 底层 Gnosis AI 系统
    /// </summary>
    public IAISystem? System => _aiSystem;

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => _initialized;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 AI 游戏系统
    /// </summary>
    public AIGameSystem()
    {
    }

    #endregion

    #region ISystem 实现

    /// <summary>
    /// 初始化 AI 系统
    /// </summary>
    public void Initialize()
    {
        _aiSystem = new AISystem();
        _initialized = true;
    }

    /// <summary>
    /// 关闭 AI 系统
    /// </summary>
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

    /// <summary>
    /// 设置系统所属的 World
    /// </summary>
    public void SetWorld(GnosisWorld world)
    {
        _world = world;
    }

    #endregion

    #region ISystem.Update

    /// <summary>
    /// 帧更新
    /// </summary>
    /// <param name="delta">帧间隔时间（秒）</param>
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

    /// <summary>
    /// 创建行为树
    /// </summary>
    /// <param name="name">行为树名称</param>
    /// <param name="blackboard">黑板（可选）</param>
    /// <returns>行为树实例</returns>
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

    /// <summary>
    /// 获取行为树
    /// </summary>
    /// <param name="name">行为树名称</param>
    /// <returns>行为树实例</returns>
    public IBehaviorTree? GetBehaviorTree(string name)
    {
        return _behaviorTrees.GetValueOrDefault(name);
    }

    /// <summary>
    /// 启动行为树
    /// </summary>
    /// <param name="name">行为树名称</param>
    public void StartBehaviorTree(string name)
    {
        if (_behaviorTrees.TryGetValue(name, out var tree))
        {
            tree.Start();
        }
    }

    /// <summary>
    /// 停止行为树
    /// </summary>
    /// <param name="name">行为树名称</param>
    public void StopBehaviorTree(string name)
    {
        if (_behaviorTrees.TryGetValue(name, out var tree))
        {
            tree.Stop();
        }
    }

    /// <summary>
    /// 创建 AI 控制器
    /// </summary>
    /// <param name="name">控制器名称</param>
    /// <returns>控制器实例</returns>
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

    /// <summary>
    /// 销毁 AI 控制器
    /// </summary>
    /// <param name="name">控制器名称</param>
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

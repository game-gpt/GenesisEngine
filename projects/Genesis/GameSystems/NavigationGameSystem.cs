using System.Numerics;
using Genesis.Core;
using Genesis.HAL;
using Genesis.Integration.Navigation;
using Genesis.Integration.Rendering;
using Gnosis.Core.Math;
using Gnosis.Navigation.NavMesh;
using Gnosis.Navigation.Path;
using Gnosis.Navigation.Query;
using ISystem = Gnosis.ECS.System.ISystem;
using IWorldSystem = Gnosis.ECS.System.IWorldSystem;
using SystemPhase = Gnosis.ECS.System.SystemPhase;
using GnosisWorld = Gnosis.ECS.World.World;

namespace Genesis.GameSystems;

/// <summary>
/// 统一导航游戏系统
/// 合并 GenesisNavigationSystem（3D）和 Genesis2DNavigationSystem（2D）
/// 2D 方法作为便捷重载，内部 Z=0
/// </summary>
public sealed class NavigationGameSystem : ISystem, IWorldSystem
{
    #region 字段

    private GnosisWorld? _world;
    private readonly NavigationSystem _navigationSystem;
    private readonly Dictionary<uint, IPath> _agentPaths = new();
    private readonly Dictionary<string, INavMeshQuery> _navMeshQueries = new();
    private bool _initialized;

    #endregion

    #region 属性

    /// <summary>
    /// 系统执行阶段
    /// </summary>
    public SystemPhase Phase => SystemPhase.Update;

    /// <summary>
    /// 底层 Gnosis 导航系统
    /// </summary>
    public NavigationSystem GnosisNavigationSystem => _navigationSystem;

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => _initialized;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化导航游戏系统
    /// </summary>
    public NavigationGameSystem()
    {
        _navigationSystem = new NavigationSystem();
    }

    #endregion

    #region ISystem 实现

    /// <summary>
    /// 初始化导航系统
    /// </summary>
    public void Initialize()
    {
        _initialized = true;
    }

    /// <summary>
    /// 关闭导航系统
    /// </summary>
    public void Shutdown()
    {
        _agentPaths.Clear();
        _navMeshQueries.Clear();
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
        if (_world is null || !_initialized)
        {
            return;
        }

        SyncNavMeshesFromWorld();
        UpdateAgentPaths();
        MoveAgentsAlongPath(delta);
        _navigationSystem.Update(delta);
    }

    #endregion

    #region 3D 公开方法

    /// <summary>
    /// 3D 寻路
    /// </summary>
    public IPath? FindPath(float startX, float startY, float startZ, float endX, float endY, float endZ)
    {
        return _navigationSystem.Pathfinder.FindPath(
            new Vector3(startX, startY, startZ),
            new Vector3(endX, endY, endZ));
    }

    /// <summary>
    /// 构建导航网格
    /// </summary>
    public void BuildNavMesh(string name, NavMeshBuildSettings settings)
    {
        _navigationSystem.BuildNavMesh(name, settings);

        var navMesh = _navigationSystem.GetNavMesh(name);

        if (navMesh is not null)
        {
            _navMeshQueries[name] = _navigationSystem.CreateQuery(navMesh);
        }
    }

    /// <summary>
    /// 获取导航网格
    /// </summary>
    public INavMesh? GetNavMesh(string name)
    {
        return _navigationSystem.GetNavMesh(name);
    }

    /// <summary>
    /// 获取导航查询
    /// </summary>
    public INavMeshQuery? GetQuery(string name)
    {
        return _navMeshQueries.GetValueOrDefault(name);
    }

    #endregion

    #region 2D 便捷方法

    /// <summary>
    /// 2D 寻路（Z=0 平面）
    /// </summary>
    public IPath? FindPath2D(float startX, float startY, float endX, float endY)
    {
        return FindPath(startX, startY, 0, endX, endY, 0);
    }

    #endregion

    #region 私有方法

    private void SyncNavMeshesFromWorld()
    {
        if (_world is null)
        {
            return;
        }

        var entities = _world.CreateQuery()
            .All<NavMeshRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!_world.HasComponent<NavMeshRef>(entityId))
            {
                continue;
            }

            var meshRef = _world.GetComponent<NavMeshRef>(entityId);

            if (!meshRef.IsValid || _navigationSystem.GetNavMesh(meshRef.MeshName) is not null)
            {
                continue;
            }

            var settings = new NavMeshBuildSettings
            {
                AgentRadius = meshRef.AgentRadius,
                AgentHeight = meshRef.AgentHeight,
                StepHeight = meshRef.StepHeight,
                SlopeAngle = meshRef.SlopeAngle,
                VoxelSize = meshRef.VoxelSize,
                RegionMinArea = meshRef.RegionMinArea
            };

            BuildNavMesh(meshRef.MeshName, settings);
        }
    }

    private void UpdateAgentPaths()
    {
        if (_world is null)
        {
            return;
        }

        var entities = _world.CreateQuery()
            .All<Transform3D>()
            .All<NavAgentRef>()
            .All<NavTargetRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!_world.HasComponent<Transform3D>(entityId) ||
                !_world.HasComponent<NavAgentRef>(entityId) ||
                !_world.HasComponent<NavTargetRef>(entityId))
            {
                continue;
            }

            var transform = _world.GetComponent<Transform3D>(entityId);
            var targetRef = _world.GetComponent<NavTargetRef>(entityId);

            if (!targetRef.HasTarget)
            {
                _agentPaths.Remove(entityId.Index);
                continue;
            }

            var path = _navigationSystem.Pathfinder.FindPath(
                new Vector3(transform.PosX, transform.PosY, transform.PosZ),
                new Vector3(targetRef.TargetX, targetRef.TargetY, targetRef.TargetZ));

            if (path is not null && path.IsComplete)
            {
                _agentPaths[entityId.Index] = path;
            }
        }
    }

    private void MoveAgentsAlongPath(float delta)
    {
        if (_world is null)
        {
            return;
        }

        var entities = _world.CreateQuery()
            .All<Transform3D>()
            .All<NavAgentRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!_agentPaths.TryGetValue(entityId.Index, out var path))
            {
                continue;
            }

            if (!_world.HasComponent<Transform3D>(entityId) || !_world.HasComponent<NavAgentRef>(entityId))
            {
                continue;
            }

            var transform = _world.GetComponent<Transform3D>(entityId);
            var agentRef = _world.GetComponent<NavAgentRef>(entityId);

            if (path.IsAtPathEnd())
            {
                _agentPaths.Remove(entityId.Index);
                continue;
            }

            var waypoint = path.CurrentWaypoint;
            var dx = waypoint.X - transform.PosX;
            var dy = waypoint.Y - transform.PosY;
            var dz = waypoint.Z - transform.PosZ;
            var distSq = dx * dx + dy * dy + dz * dz;
            var stoppingDistSq = agentRef.StoppingDistance * agentRef.StoppingDistance;

            if (distSq <= stoppingDistSq)
            {
                path.Advance();

                if (path.IsAtPathEnd())
                {
                    _agentPaths.Remove(entityId.Index);
                }

                continue;
            }

            var dist = MathF.Sqrt(distSq);
            var moveSpeed = agentRef.Speed * delta;
            var ratio = MathF.Min(moveSpeed / dist, 1f);

            var newX = transform.PosX + dx * ratio;
            var newY = transform.PosY + dy * ratio;
            var newZ = transform.PosZ + dz * ratio;

            var newTransform = transform.WithPosition(newX, newY, newZ);
            _world.SetComponent(entityId, newTransform);
        }
    }

    #endregion
}

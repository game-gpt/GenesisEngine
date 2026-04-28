using System.Numerics;
using Genesis.Integration.Navigation;
using Genesis.Integration.Rendering;
using Gnosis.Core.Math;
using Gnosis.ECS.System;
using Gnosis.ECS.World;
using Gnosis.Navigation.NavMesh;
using Gnosis.Navigation.Path;
using Gnosis.Navigation.Query;

namespace Genesis.Integration.Navigation2D;

[Obsolete("请使用 Genesis.GameSystems.NavigationGameSystem 替代。Integration 层将在未来版本移除。")]
public sealed class Genesis2DNavigationSystem : ISystem, IWorldSystem
{
    #region 字段

    private Gnosis.ECS.World.World? _world;
    private readonly NavigationSystem _navigationSystem;
    private readonly Dictionary<uint, IPath> _agentPaths = new();
    private readonly Dictionary<string, INavMeshQuery> _navMeshQueries = new();
    private bool _initialized;

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.Update;

    public NavigationSystem GnosisNavigationSystem => _navigationSystem;

    public bool IsInitialized => _initialized;

    #endregion

    #region 构造函数

    public Genesis2DNavigationSystem()
    {
        _navigationSystem = new NavigationSystem();
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _initialized = true;
        Console.WriteLine("[Genesis] 2D 导航系统初始化完成");
    }

    public void Shutdown()
    {
        _agentPaths.Clear();
        _navMeshQueries.Clear();
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

    #region 公开方法

    public IPath? FindPath(float startX, float startY, float endX, float endY)
    {
        return _navigationSystem.Pathfinder.FindPath(
            new Vector3(startX, startY, 0),
            new Vector3(endX, endY, 0));
    }

    public void BuildNavMesh(string name, NavMeshBuildSettings settings)
    {
        _navigationSystem.BuildNavMesh(name, settings);

        var navMesh = _navigationSystem.GetNavMesh(name);

        if (navMesh is not null)
        {
            _navMeshQueries[name] = _navigationSystem.CreateQuery(navMesh);
        }
    }

    public INavMesh? GetNavMesh(string name)
    {
        return _navigationSystem.GetNavMesh(name);
    }

    public INavMeshQuery? GetQuery(string name)
    {
        return _navMeshQueries.GetValueOrDefault(name);
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
                new Vector3(transform.PosX, transform.PosY, 0),
                new Vector3(targetRef.TargetX, targetRef.TargetY, 0));

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
            var distSq = dx * dx + dy * dy;
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

            var newTransform = transform.WithPosition(newX, newY, transform.PosZ);
            _world.SetComponent(entityId, newTransform);
        }
    }

    #endregion
}

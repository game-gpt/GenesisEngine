using System.Numerics;
using Genesis.Core;
using Genesis.GameSystems.Components;
using Genesis.HAL;
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
/// 通过 HAL 接口访问实体世界
/// </summary>
public sealed class NavigationGameSystem : ISystem, IWorldSystem
{
    #region 字段

    private GnosisWorld? _world;
    private IEntityWorld? _entityWorld;
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

    public NavigationGameSystem()
    {
        _navigationSystem = new NavigationSystem();
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _initialized = true;
    }

    public void Shutdown()
    {
        _agentPaths.Clear();
        _navMeshQueries.Clear();
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
        if (!_initialized)
        {
            return;
        }

        var ew = _entityWorld;
        if (ew is not null)
        {
            SyncNavMeshesFromEntityWorld(ew);
            UpdateAgentPathsFromEntityWorld(ew);
            MoveAgentsAlongPathFromEntityWorld(ew, delta);
        }
        else if (_world is not null)
        {
            SyncNavMeshesFromWorld();
            UpdateAgentPaths();
            MoveAgentsAlongPath(delta);
        }

        _navigationSystem.Update(delta);
    }

    #endregion

    #region 3D 公开方法

    public IPath? FindPath(float startX, float startY, float startZ, float endX, float endY, float endZ)
    {
        return _navigationSystem.Pathfinder.FindPath(
            new Vector3(startX, startY, startZ),
            new Vector3(endX, endY, endZ));
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

    #region 2D 便捷方法

    public IPath? FindPath2D(float startX, float startY, float endX, float endY)
    {
        return FindPath(startX, startY, 0, endX, endY, 0);
    }

    #endregion

    #region IEntityWorld 路径

    private void SyncNavMeshesFromEntityWorld(IEntityWorld ew)
    {
        var entities = ew.CreateQuery()
            .All<NavMeshRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!ew.HasComponent<NavMeshRef>(entityId))
            {
                continue;
            }

            var meshRef = ew.GetComponent<NavMeshRef>(entityId);

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

    private void UpdateAgentPathsFromEntityWorld(IEntityWorld ew)
    {
        var entities = ew.CreateQuery()
            .All<Transform3D>()
            .All<NavAgentRef>()
            .All<NavTargetRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!ew.HasComponent<Transform3D>(entityId) ||
                !ew.HasComponent<NavAgentRef>(entityId) ||
                !ew.HasComponent<NavTargetRef>(entityId))
            {
                continue;
            }

            var transform = ew.GetComponent<Transform3D>(entityId);
            var targetRef = ew.GetComponent<NavTargetRef>(entityId);

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

    private void MoveAgentsAlongPathFromEntityWorld(IEntityWorld ew, float delta)
    {
        var entities = ew.CreateQuery()
            .All<Transform3D>()
            .All<NavAgentRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!_agentPaths.TryGetValue(entityId.Index, out var path))
            {
                continue;
            }

            if (!ew.HasComponent<Transform3D>(entityId) || !ew.HasComponent<NavAgentRef>(entityId))
            {
                continue;
            }

            var transform = ew.GetComponent<Transform3D>(entityId);
            var agentRef = ew.GetComponent<NavAgentRef>(entityId);

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
            ew.SetComponent(entityId, newTransform);
        }
    }

    #endregion

    #region GnosisWorld 回退路径

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

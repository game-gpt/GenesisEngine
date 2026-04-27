using System.Numerics;
using Genesis.Integration.Rendering;
using Gnosis.ECS.System;
using Gnosis.ECS.World;
using Gnosis.Physics.Collision;
using Gnosis.Physics.Dynamics;
using Gnosis.Physics.ECS;
using Gnosis.Physics.Query;

namespace Genesis.Integration.Physics2D;

public sealed class Genesis2DPhysicsSystem : ISystem, IWorldSystem
{
    #region 字段

    private Gnosis.ECS.World.World? _world;
    private PhysicsEcsSystem? _inner;
    private bool _isInitialized;

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.Update;

    public Vector2 Gravity2D
    {
        get => _inner != null
            ? new Vector2(_inner.PhysicsWorld.Gravity.X, _inner.PhysicsWorld.Gravity.Y)
            : new Vector2(0, -9.81f);
        set
        {
            if (_inner != null)
            {
                _inner.PhysicsWorld.Gravity = new Vector3(value.X, value.Y, 0);
            }
        }
    }

    public bool IsInitialized => _isInitialized;

    public IPhysicsWorld PhysicsWorld => _inner?.PhysicsWorld
        ?? throw new InvalidOperationException("2D 物理系统未初始化");

    public PhysicsEcsSystem? Inner => _inner;

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _inner = new PhysicsEcsSystem();
        _inner.PhysicsWorld.Gravity = new Vector3(0, -9.81f, 0);
        _isInitialized = true;
        Console.WriteLine("[Genesis] 2D 物理系统初始化完成 - 重力: -9.81");
    }

    public void Shutdown()
    {
        _inner?.Shutdown();
        _inner = null;
        _isInitialized = false;
    }

    #endregion

    #region IWorldSystem 实现

    public void SetWorld(Gnosis.ECS.World.World world)
    {
        _world = world;
        _inner?.SetWorld(world);
    }

    #endregion

    #region ISystem.Update

    public void Update(float delta)
    {
        if (!_isInitialized || _inner is null)
        {
            return;
        }

        _inner.Update(delta);
    }

    #endregion

    #region 公开方法

    public IRaycastResult Raycast2D(Vector2 origin, Vector2 direction, float maxDistance)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("2D 物理系统未初始化");
        }

        return _inner.Raycast(
            new Vector3(origin.X, origin.Y, 0),
            new Vector3(direction.X, direction.Y, 0),
            maxDistance);
    }

    public IRaycastResult[] RaycastAll2D(Vector2 origin, Vector2 direction, float maxDistance)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("2D 物理系统未初始化");
        }

        return _inner.RaycastAll(
            new Vector3(origin.X, origin.Y, 0),
            new Vector3(direction.X, direction.Y, 0),
            maxDistance);
    }

    public IOverlapResult OverlapCircle(Vector2 center, float radius)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("2D 物理系统未初始化");
        }

        return _inner.OverlapSphere(new Vector3(center.X, center.Y, 0), radius);
    }

    public IOverlapResult OverlapBox2D(Vector2 center, Vector2 halfExtents)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("2D 物理系统未初始化");
        }

        return _inner.OverlapBox(
            new Vector3(center.X, center.Y, 0),
            new Vector3(halfExtents.X, halfExtents.Y, 0.01f));
    }

    #endregion
}

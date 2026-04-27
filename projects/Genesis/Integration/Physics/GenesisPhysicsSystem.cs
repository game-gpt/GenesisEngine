using System.Numerics;
using Genesis.Integration.Rendering;
using Gnosis.Core.Entity;
using Gnosis.ECS.World;
using Gnosis.Physics.Collision;
using Gnosis.Physics.Dynamics;
using Gnosis.Physics.ECS;
using Gnosis.Physics.Query;
using ISystem = Gnosis.ECS.System.ISystem;
using IWorldSystem = Gnosis.ECS.System.IWorldSystem;
using SystemPhase = Gnosis.ECS.System.SystemPhase;

namespace Genesis.Integration.Physics;

public sealed class GenesisPhysicsSystem : ISystem, IWorldSystem
{
    #region 字段

    private Gnosis.ECS.World.World? _world;
    private PhysicsEcsSystem? _inner;
    private bool _isInitialized;
    private float _gravityX;
    private float _gravityY;
    private float _gravityZ;

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.Update;

    public Vector3 Gravity
    {
        get => new(_gravityX, _gravityY, _gravityZ);
        set
        {
            _gravityX = value.X;
            _gravityY = value.Y;
            _gravityZ = value.Z;
            if (_inner != null)
            {
                _inner.PhysicsWorld.Gravity = value;
            }
        }
    }

    public bool IsInitialized => _isInitialized;

    public IPhysicsWorld PhysicsWorld => _inner?.PhysicsWorld
        ?? throw new InvalidOperationException("物理系统未初始化");

    public PhysicsEcsSystem? Inner => _inner;

    #endregion

    #region 构造函数

    public GenesisPhysicsSystem()
    {
        _gravityX = 0;
        _gravityY = -9.81f;
        _gravityZ = 0;
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _inner = new PhysicsEcsSystem();
        _inner.PhysicsWorld.Gravity = Gravity;
        _isInitialized = true;
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

    public void SetGravity(float x, float y, float z)
    {
        Gravity = new Vector3(x, y, z);
    }

    public IRaycastResult Raycast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("物理系统未初始化");
        }

        return _inner.Raycast(origin, direction, maxDistance);
    }

    public IRaycastResult[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("物理系统未初始化");
        }

        return _inner.RaycastAll(origin, direction, maxDistance);
    }

    public IOverlapResult OverlapSphere(Vector3 center, float radius)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("物理系统未初始化");
        }

        return _inner.OverlapSphere(center, radius);
    }

    public IOverlapResult OverlapBox(Vector3 center, Vector3 halfExtents)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("物理系统未初始化");
        }

        return _inner.OverlapBox(center, halfExtents);
    }

    #endregion
}

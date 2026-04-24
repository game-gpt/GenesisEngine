using System.Numerics;
using Gnosis.Core.Math;
using Gnosis.Physics.Dynamics;
using Gnosis.Physics.Query;
using Gnosis.Physics.Shape;

namespace Genesis.Integration;

public sealed class Physics2DIntegration : IDisposable
{
    #region 常量

    private const float DefaultGravityY = -9.81f;

    #endregion

    #region 字段

    private IPhysicsSystem? _physicsSystem;
    private IPhysicsWorld? _physicsWorld;
    private bool _disposed;

    #endregion

    #region 属性

    public IPhysicsSystem? System => _physicsSystem;

    public IPhysicsWorld? World => _physicsWorld;

    public bool IsInitialized => _physicsWorld is not null;

    #endregion

    #region 初始化

    public void Initialize(IPhysicsSystem? physicsSystem = null)
    {
        _physicsSystem = physicsSystem ?? new Gnosis.Physics.Dynamics.PhysicsSystem();
        _physicsWorld = _physicsSystem.CreateWorld();
        _physicsWorld.Gravity = new Vector3(0f, DefaultGravityY, 0f);

        Console.WriteLine($"[Genesis] 2D 物理初始化完成 - 重力: {DefaultGravityY}");
    }

    #endregion

    #region 刚体管理

    public IRigidBody CreateDynamicBody(string name, Vector3 position)
    {
        if (_physicsWorld is null)
        {
            throw new InvalidOperationException("物理系统未初始化，请先调用 Initialize()");
        }

        var body = _physicsWorld.CreateRigidBody(name, RigidBodyType.Dynamic);
        body.Position = position;
        return body;
    }

    public IRigidBody CreateStaticBody(string name, Vector3 position)
    {
        if (_physicsWorld is null)
        {
            throw new InvalidOperationException("物理系统未初始化，请先调用 Initialize()");
        }

        var body = _physicsWorld.CreateRigidBody(name, RigidBodyType.Static);
        body.Position = position;
        return body;
    }

    public IRigidBody CreateKinematicBody(string name, Vector3 position)
    {
        if (_physicsWorld is null)
        {
            throw new InvalidOperationException("物理系统未初始化，请先调用 Initialize()");
        }

        var body = _physicsWorld.CreateRigidBody(name, RigidBodyType.Kinematic);
        body.Position = position;
        return body;
    }

    public void DestroyBody(IRigidBody body)
    {
        _physicsWorld?.DestroyRigidBody(body);
    }

    #endregion

    #region 碰撞体

    public IBoxCollider CreateBoxCollider(string name, float halfExtentsX, float halfExtentsY, float halfExtentsZ = 0.5f)
    {
        if (_physicsWorld is null)
        {
            throw new InvalidOperationException("物理系统未初始化，请先调用 Initialize()");
        }

        var collider = _physicsWorld.CreateBoxCollider(name);

        if (collider is BoxCollider box)
        {
            box.HalfExtentsX = halfExtentsX;
            box.HalfExtentsY = halfExtentsY;
            box.HalfExtentsZ = halfExtentsZ;
        }

        return collider;
    }

    public void AttachCollider(IRigidBody body, ICollider collider)
    {
        _physicsWorld?.AttachCollider(body, collider);
    }

    public void DetachCollider(IRigidBody body, ICollider collider)
    {
        _physicsWorld?.DetachCollider(body, collider);
    }

    #endregion

    #region 查询

    public IRaycastResult Raycast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        if (_physicsWorld is null)
        {
            throw new InvalidOperationException("物理系统未初始化，请先调用 Initialize()");
        }

        return _physicsWorld.Raycast(origin, direction, maxDistance);
    }

    #endregion

    #region 更新

    public void Update(float delta)
    {
        if (_physicsWorld is not null)
        {
            _physicsWorld.Step(delta);
            _physicsWorld.SyncTransforms();
        }

        _physicsSystem?.Update(delta);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_physicsSystem is not null && _physicsWorld is not null)
        {
            _physicsSystem.DestroyWorld(_physicsWorld);
        }

        _disposed = true;
    }

    #endregion
}

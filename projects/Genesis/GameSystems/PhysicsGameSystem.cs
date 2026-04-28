using System.Numerics;
using Genesis.Core;
using Genesis.GameSystems.Components;
using Genesis.HAL;
using Gnosis.Physics.Collision;
using Gnosis.Physics.Dynamics;
using Gnosis.Physics.ECS;
using Gnosis.Physics.Query;
using ISystem = Gnosis.ECS.System.ISystem;
using IWorldSystem = Gnosis.ECS.System.IWorldSystem;
using SystemPhase = Gnosis.ECS.System.SystemPhase;
using GnosisWorld = Gnosis.ECS.World.World;

namespace Genesis.GameSystems;

/// <summary>
/// 统一物理游戏系统
/// 合并 GenesisPhysicsSystem（3D）和 Genesis2DPhysicsSystem（2D）
/// 2D 方法作为便捷重载，内部映射到 3D（Z=0）
/// 通过 HAL 接口访问实体世界
/// </summary>
public sealed class PhysicsGameSystem : ISystem, IWorldSystem
{
    #region 字段

    private GnosisWorld? _world;
    private IEntityWorld? _entityWorld;
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

    public Vector2 Gravity2D
    {
        get => new(_gravityX, _gravityY);
        set => Gravity = new Vector3(value.X, value.Y, 0);
    }

    public bool IsInitialized => _isInitialized;

    public IPhysicsWorld PhysicsWorld => _inner?.PhysicsWorld
        ?? throw new InvalidOperationException("物理系统未初始化");

    public PhysicsEcsSystem? Inner => _inner;

    #endregion

    #region 构造函数

    public PhysicsGameSystem(bool gravity2D = false)
    {
        _gravityX = 0;
        _gravityY = -9.81f;
        _gravityZ = gravity2D ? 0 : 0;
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

    public void SetWorld(GnosisWorld world)
    {
        _world = world;
        _inner?.SetWorld(world);
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
        if (!_isInitialized || _inner is null)
        {
            return;
        }

        _inner.Update(delta);
    }

    #endregion

    #region 3D 公开方法

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

    #region 2D 便捷方法

    public IRaycastResult Raycast2D(Vector2 origin, Vector2 direction, float maxDistance)
    {
        return Raycast(
            new Vector3(origin, 0),
            new Vector3(direction, 0),
            maxDistance);
    }

    public IRaycastResult[] RaycastAll2D(Vector2 origin, Vector2 direction, float maxDistance)
    {
        return RaycastAll(
            new Vector3(origin, 0),
            new Vector3(direction, 0),
            maxDistance);
    }

    public IOverlapResult OverlapCircle(Vector2 center, float radius)
    {
        return OverlapSphere(new Vector3(center, 0), radius);
    }

    public IOverlapResult OverlapBox2D(Vector2 center, Vector2 halfExtents)
    {
        return OverlapBox(new Vector3(center, 0), new Vector3(halfExtents, 0.01f));
    }

    #endregion
}

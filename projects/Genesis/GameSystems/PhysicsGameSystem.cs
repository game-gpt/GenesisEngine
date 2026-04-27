using System.Numerics;
using Genesis.Core;
using Genesis.HAL;
using Genesis.Integration.Rendering;
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
/// </summary>
public sealed class PhysicsGameSystem : ISystem, IWorldSystem
{
    #region 字段

    private GnosisWorld? _world;
    private PhysicsEcsSystem? _inner;
    private bool _isInitialized;
    private float _gravityX;
    private float _gravityY;
    private float _gravityZ;

    #endregion

    #region 属性

    /// <summary>
    /// 系统执行阶段
    /// </summary>
    public SystemPhase Phase => SystemPhase.Update;

    /// <summary>
    /// 重力向量
    /// </summary>
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

    /// <summary>
    /// 2D 重力（Z 分量强制为 0）
    /// </summary>
    public Vector2 Gravity2D
    {
        get => new(_gravityX, _gravityY);
        set => Gravity = new Vector3(value.X, value.Y, 0);
    }

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// 物理世界
    /// </summary>
    public IPhysicsWorld PhysicsWorld => _inner?.PhysicsWorld
        ?? throw new InvalidOperationException("物理系统未初始化");

    /// <summary>
    /// 底层 Gnosis 物理系统
    /// </summary>
    public PhysicsEcsSystem? Inner => _inner;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化物理游戏系统
    /// </summary>
    /// <param name="gravity2D">是否使用 2D 重力（默认 Y=-9.81，Z=0）</param>
    public PhysicsGameSystem(bool gravity2D = false)
    {
        _gravityX = 0;
        _gravityY = -9.81f;
        _gravityZ = gravity2D ? 0 : 0;
    }

    #endregion

    #region ISystem 实现

    /// <summary>
    /// 初始化物理系统
    /// </summary>
    public void Initialize()
    {
        _inner = new PhysicsEcsSystem();
        _inner.PhysicsWorld.Gravity = Gravity;
        _isInitialized = true;
    }

    /// <summary>
    /// 关闭物理系统
    /// </summary>
    public void Shutdown()
    {
        _inner?.Shutdown();
        _inner = null;
        _isInitialized = false;
    }

    #endregion

    #region IWorldSystem 实现

    /// <summary>
    /// 设置系统所属的 World
    /// </summary>
    public void SetWorld(GnosisWorld world)
    {
        _world = world;
        _inner?.SetWorld(world);
    }

    #endregion

    #region ISystem.Update

    /// <summary>
    /// 帧更新
    /// </summary>
    /// <param name="delta">帧间隔时间（秒）</param>
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

    /// <summary>
    /// 设置重力
    /// </summary>
    public void SetGravity(float x, float y, float z)
    {
        Gravity = new Vector3(x, y, z);
    }

    /// <summary>
    /// 3D 射线检测
    /// </summary>
    public IRaycastResult Raycast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("物理系统未初始化");
        }

        return _inner.Raycast(origin, direction, maxDistance);
    }

    /// <summary>
    /// 3D 射线检测全部
    /// </summary>
    public IRaycastResult[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("物理系统未初始化");
        }

        return _inner.RaycastAll(origin, direction, maxDistance);
    }

    /// <summary>
    /// 球体重叠检测
    /// </summary>
    public IOverlapResult OverlapSphere(Vector3 center, float radius)
    {
        if (_inner is null)
        {
            throw new InvalidOperationException("物理系统未初始化");
        }

        return _inner.OverlapSphere(center, radius);
    }

    /// <summary>
    /// 盒体重叠检测
    /// </summary>
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

    /// <summary>
    /// 2D 射线检测（Z=0 平面）
    /// </summary>
    public IRaycastResult Raycast2D(Vector2 origin, Vector2 direction, float maxDistance)
    {
        return Raycast(
            new Vector3(origin, 0),
            new Vector3(direction, 0),
            maxDistance);
    }

    /// <summary>
    /// 2D 射线检测全部（Z=0 平面）
    /// </summary>
    public IRaycastResult[] RaycastAll2D(Vector2 origin, Vector2 direction, float maxDistance)
    {
        return RaycastAll(
            new Vector3(origin, 0),
            new Vector3(direction, 0),
            maxDistance);
    }

    /// <summary>
    /// 圆形重叠检测（Z=0 平面）
    /// </summary>
    public IOverlapResult OverlapCircle(Vector2 center, float radius)
    {
        return OverlapSphere(new Vector3(center, 0), radius);
    }

    /// <summary>
    /// 2D 盒体重叠检测（Z=0 平面，深度 0.01）
    /// </summary>
    public IOverlapResult OverlapBox2D(Vector2 center, Vector2 halfExtents)
    {
        return OverlapBox(new Vector3(center, 0), new Vector3(halfExtents, 0.01f));
    }

    #endregion
}

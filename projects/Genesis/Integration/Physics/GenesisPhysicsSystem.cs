using System.Numerics;
using Genesis.Integration.Rendering;
using Gnosis.Core;
using Gnosis.ECS.World;
using Gnosis.Physics.Dynamics;
using ISystem = Gnosis.ECS.System.ISystem;
using IWorldSystem = Gnosis.ECS.System.IWorldSystem;
using SystemPhase = Gnosis.ECS.System.SystemPhase;

namespace Genesis.Integration.Physics;

public sealed class GenesisPhysicsSystem : ISystem, IWorldSystem
{
    #region 常量

    private const float DefaultGravityY = -9.81f;
    private const float DefaultFixedDeltaTime = 1f / 60f;

    #endregion

    #region 字段

    private Gnosis.ECS.World.World? _world;
    private bool _isInitialized;
    private float _gravityX;
    private float _gravityY;
    private float _gravityZ;
    private float _fixedDeltaTime;
    private float _accumulator;

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
        }
    }

    public bool IsInitialized => _isInitialized;

    #endregion

    #region 构造函数

    public GenesisPhysicsSystem()
    {
        _gravityX = 0;
        _gravityY = DefaultGravityY;
        _gravityZ = 0;
        _fixedDeltaTime = DefaultFixedDeltaTime;
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _isInitialized = true;
    }

    public void Shutdown()
    {
        _isInitialized = false;
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
        if (!_isInitialized || _world is null)
        {
            return;
        }

        _accumulator += delta;

        while (_accumulator >= _fixedDeltaTime)
        {
            Step(_fixedDeltaTime);
            _accumulator -= _fixedDeltaTime;
        }
    }

    #endregion

    #region 公开方法

    public void SetGravity(float x, float y, float z)
    {
        _gravityX = x;
        _gravityY = y;
        _gravityZ = z;
    }

    #endregion

    #region 私有方法

    private void Step(float dt)
    {
        if (_world is null)
        {
            return;
        }

        ApplyGravity(dt);
        IntegrateVelocities(dt);
        ResolveCollisions();
    }

    private void ApplyGravity(float dt)
    {
        if (_world is null)
        {
            return;
        }

        var dynamicEntities = _world.CreateQuery()
            .All<Transform3D>()
            .All<RigidBodyRef>()
            .Build();

        foreach (var entityId in dynamicEntities)
        {
            if (!_world.HasComponent<Transform3D>(entityId) || !_world.HasComponent<RigidBodyRef>(entityId))
            {
                continue;
            }

            var rb = _world.GetComponent<RigidBodyRef>(entityId);
            if (rb.BodyType != RigidBodyType.Dynamic || !rb.UseGravity)
            {
                continue;
            }

            var velocity = _world.HasComponent<PhysicsVelocity>(entityId)
                ? _world.GetComponent<PhysicsVelocity>(entityId)
                : PhysicsVelocity.Zero;

            velocity = velocity with
            {
                Vx = velocity.Vx + _gravityX * dt,
                Vy = velocity.Vy + _gravityY * dt,
                Vz = velocity.Vz + _gravityZ * dt
            };

            if (!_world.HasComponent<PhysicsVelocity>(entityId))
            {
                _world.AddComponent(entityId, velocity);
            }
            else
            {
                _world.SetComponent(entityId, velocity);
            }
        }
    }

    private void IntegrateVelocities(float dt)
    {
        if (_world is null)
        {
            return;
        }

        var dynamicEntities = _world.CreateQuery()
            .All<Transform3D>()
            .All<PhysicsVelocity>()
            .Build();

        foreach (var entityId in dynamicEntities)
        {
            if (!_world.HasComponent<Transform3D>(entityId) || !_world.HasComponent<PhysicsVelocity>(entityId))
            {
                continue;
            }

            var transform = _world.GetComponent<Transform3D>(entityId);
            var velocity = _world.GetComponent<PhysicsVelocity>(entityId);

            var newTransform = transform.WithPosition(
                transform.PosX + velocity.Vx * dt,
                transform.PosY + velocity.Vy * dt,
                transform.PosZ + velocity.Vz * dt);

            _world.SetComponent(entityId, newTransform);
        }
    }

    private void ResolveCollisions()
    {
        if (_world is null)
        {
            return;
        }

        var entitiesWithColliders = _world.CreateQuery()
            .All<Transform3D>()
            .All<ColliderRef>()
            .Build();

        var entityList = entitiesWithColliders.ToList();
        for (var i = 0; i < entityList.Count; i++)
        {
            for (var j = i + 1; j < entityList.Count; j++)
            {
                ResolvePairCollision(entityList[i], entityList[j]);
            }
        }
    }

    private void ResolvePairCollision(EntityId entityA, EntityId entityB)
    {
        if (_world is null)
        {
            return;
        }

        if (!_world.HasComponent<Transform3D>(entityA) || !_world.HasComponent<ColliderRef>(entityA) ||
            !_world.HasComponent<Transform3D>(entityB) || !_world.HasComponent<ColliderRef>(entityB))
        {
            return;
        }

        var transformA = _world.GetComponent<Transform3D>(entityA);
        var colliderA = _world.GetComponent<ColliderRef>(entityA);
        var transformB = _world.GetComponent<Transform3D>(entityB);
        var colliderB = _world.GetComponent<ColliderRef>(entityB);

        if (colliderA.IsTrigger || colliderB.IsTrigger)
        {
            return;
        }

        if (colliderA.ShapeType == ColliderShapeType.Box && colliderB.ShapeType == ColliderShapeType.Box)
        {
            ResolveAABB(entityA, transformA, colliderA, entityB, transformB, colliderB);
        }
    }

    private void ResolveAABB(
        EntityId entityA, Transform3D transformA, ColliderRef colliderA,
        EntityId entityB, Transform3D transformB, ColliderRef colliderB)
    {
        if (_world is null)
        {
            return;
        }

        var dx = transformB.PosX - transformA.PosX;
        var dy = transformB.PosY - transformA.PosY;
        var dz = transformB.PosZ - transformA.PosZ;

        var overlapX = colliderA.HalfExtentsX + colliderB.HalfExtentsX - MathF.Abs(dx);
        var overlapY = colliderA.HalfExtentsY + colliderB.HalfExtentsY - MathF.Abs(dy);
        var overlapZ = colliderA.HalfExtentsZ + colliderB.HalfExtentsZ - MathF.Abs(dz);

        if (overlapX <= 0 || overlapY <= 0 || overlapZ <= 0)
        {
            return;
        }

        var hasVelA = _world.HasComponent<PhysicsVelocity>(entityA);
        var hasVelB = _world.HasComponent<PhysicsVelocity>(entityB);
        var hasRbA = _world.HasComponent<RigidBodyRef>(entityA);
        var hasRbB = _world.HasComponent<RigidBodyRef>(entityB);

        if (!hasVelA && !hasVelB)
        {
            return;
        }

        float pushX = 0, pushY = 0, pushZ = 0;
        if (overlapX <= overlapY && overlapX <= overlapZ)
        {
            pushX = overlapX * MathF.Sign(dx);
        }
        else if (overlapY <= overlapZ)
        {
            pushY = overlapY * MathF.Sign(dy);
        }
        else
        {
            pushZ = overlapZ * MathF.Sign(dz);
        }

        if (hasVelA && hasRbA)
        {
            var rbA = _world.GetComponent<RigidBodyRef>(entityA);
            if (rbA.BodyType == RigidBodyType.Dynamic)
            {
                var tA = _world.GetComponent<Transform3D>(entityA);
                _world.SetComponent(entityA, tA.WithPosition(tA.PosX - pushX * 0.5f, tA.PosY - pushY * 0.5f, tA.PosZ - pushZ * 0.5f));

                var velA = _world.GetComponent<PhysicsVelocity>(entityA);
                if (pushX != 0) velA = velA with { Vx = 0 };
                if (pushY != 0) velA = velA with { Vy = 0 };
                if (pushZ != 0) velA = velA with { Vz = 0 };
                _world.SetComponent(entityA, velA);
            }
        }

        if (hasVelB && hasRbB)
        {
            var rbB = _world.GetComponent<RigidBodyRef>(entityB);
            if (rbB.BodyType == RigidBodyType.Dynamic)
            {
                var tB = _world.GetComponent<Transform3D>(entityB);
                _world.SetComponent(entityB, tB.WithPosition(tB.PosX + pushX * 0.5f, tB.PosY + pushY * 0.5f, tB.PosZ + pushZ * 0.5f));

                var velB = _world.GetComponent<PhysicsVelocity>(entityB);
                if (pushX != 0) velB = velB with { Vx = 0 };
                if (pushY != 0) velB = velB with { Vy = 0 };
                if (pushZ != 0) velB = velB with { Vz = 0 };
                _world.SetComponent(entityB, velB);
            }
        }
    }

    #endregion
}

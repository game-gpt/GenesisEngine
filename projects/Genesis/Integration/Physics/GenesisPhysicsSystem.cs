using System.Numerics;
using Genesis.Integration.Rendering;
using Gnosis.Core.Entity;
using Gnosis.ECS.System;
using Gnosis.ECS.World;
using Gnosis.Physics.Dynamics;
using Gnosis.Physics.Query;
using Gnosis.Physics.Shape;

namespace Genesis.Integration.Physics;

public sealed class GenesisPhysicsSystem : ISystem, IWorldSystem
{
    #region 字段

    private Gnosis.ECS.World.World? _world;
    private readonly Gnosis.Physics.Dynamics.PhysicsSystem _physicsSystem;
    private IPhysicsWorld? _physicsWorld;
    private readonly Dictionary<uint, IRigidBody> _entityBodies = new();
    private readonly Dictionary<uint, List<ICollider>> _entityColliders = new();

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.Update;

    public IPhysicsWorld? PhysicsWorld => _physicsWorld;

    public Gnosis.Physics.Dynamics.PhysicsSystem GnosisPhysicsSystem => _physicsSystem;

    #endregion

    #region 构造函数

    public GenesisPhysicsSystem()
    {
        _physicsSystem = new Gnosis.Physics.Dynamics.PhysicsSystem();
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _physicsWorld = _physicsSystem.CreateWorld();
        _physicsWorld.Gravity = new Vector3(0f, -9.81f, 0f);
    }

    public void Shutdown()
    {
        _entityColliders.Clear();
        _entityBodies.Clear();

        if (_physicsWorld is not null)
        {
            _physicsSystem.DestroyWorld(_physicsWorld);
            _physicsWorld = null;
        }
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
        if (_world is null || _physicsWorld is null)
        {
            return;
        }

        SyncNewBodiesFromWorld();
        SyncForcesFromWorld();
        _physicsSystem.Update(delta);
        SyncTransformsToWorld();
    }

    #endregion

    #region 公开方法

    public IRaycastResult Raycast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        if (_physicsWorld is null)
        {
            return new RaycastResult();
        }

        return _physicsWorld.Raycast(origin, direction, maxDistance);
    }

    public void SetGravity(Vector3 gravity)
    {
        if (_physicsWorld is not null)
        {
            _physicsWorld.Gravity = gravity;
        }
    }

    #endregion

    #region 私有方法

    private void SyncNewBodiesFromWorld()
    {
        if (_world is null || _physicsWorld is null)
        {
            return;
        }

        var entities = _world.CreateQuery()
            .All<Transform3D>()
            .All<RigidBodyRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (_entityBodies.ContainsKey(entityId.Index))
            {
                continue;
            }

            if (!_world.HasComponent<Transform3D>(entityId) || !_world.HasComponent<RigidBodyRef>(entityId))
            {
                continue;
            }

            var transform = _world.GetComponent<Transform3D>(entityId);
            var bodyRef = _world.GetComponent<RigidBodyRef>(entityId);

            var body = _physicsWorld.CreateRigidBody(bodyRef.BodyName, bodyRef.BodyType);
            body.Position = new Vector3(transform.PosX, transform.PosY, transform.PosZ);
            body.Mass = bodyRef.Mass;
            body.UseGravity = bodyRef.UseGravity;
            body.IsKinematic = bodyRef.IsKinematic;

            _entityBodies[entityId.Index] = body;

            if (_world.HasComponent<ColliderRef>(entityId))
            {
                var colliderRef = _world.GetComponent<ColliderRef>(entityId);
                ICollider? collider = colliderRef.ShapeType switch
                {
                    ColliderShapeType.Box => _physicsWorld.CreateBoxCollider($"collider_{entityId.Index}"),
                    ColliderShapeType.Sphere => _physicsWorld.CreateSphereCollider($"collider_{entityId.Index}"),
                    ColliderShapeType.Capsule => _physicsWorld.CreateCapsuleCollider($"collider_{entityId.Index}"),
                    ColliderShapeType.Mesh => _physicsWorld.CreateMeshCollider($"collider_{entityId.Index}"),
                    _ => null
                };

                if (collider is not null)
                {
                    collider.IsTrigger = colliderRef.IsTrigger;
                    _physicsWorld.AttachCollider(body, collider);

                    if (!_entityColliders.ContainsKey(entityId.Index))
                    {
                        _entityColliders[entityId.Index] = new List<ICollider>();
                    }

                    _entityColliders[entityId.Index].Add(collider);
                }
            }
        }
    }

    private void SyncForcesFromWorld()
    {
        if (_world is null)
        {
            return;
        }

        var entities = _world.CreateQuery()
            .All<RigidBodyRef>()
            .All<PhysicsVelocity>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!_entityBodies.TryGetValue(entityId.Index, out var body))
            {
                continue;
            }

            if (!_world.HasComponent<PhysicsVelocity>(entityId))
            {
                continue;
            }

            var velocity = _world.GetComponent<PhysicsVelocity>(entityId);
            body.Velocity = new Vector3(velocity.Vx, velocity.Vy, velocity.Vz);
            body.AngularVelocity = new Vector3(velocity.Avx, velocity.Avy, velocity.Avz);
        }
    }

    private void SyncTransformsToWorld()
    {
        if (_world is null)
        {
            return;
        }

        var entities = _world.CreateQuery()
            .All<Transform3D>()
            .All<RigidBodyRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!_entityBodies.TryGetValue(entityId.Index, out var body))
            {
                continue;
            }

            if (!_world.HasComponent<Transform3D>(entityId))
            {
                continue;
            }

            var pos = body.Position;

            var newTransform = _world.GetComponent<Transform3D>(entityId)
                .WithPosition(pos.X, pos.Y, pos.Z);

            _world.SetComponent(entityId, newTransform);
        }
    }

    #endregion
}

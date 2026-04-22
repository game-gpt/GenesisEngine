using Gnosis.ECS;

namespace Genesis.Terraria.ECS;

public sealed class EcsWorld : IWorld
{
    private readonly EntityManager _entityManager;
    private readonly ComponentManager _componentManager;
    private readonly List<ISystem> _systems;

    public EntityManager Entities => _entityManager;
    public ComponentManager Components => _componentManager;

    public int EntityCount => _entityManager.AliveCount;

    public EcsWorld()
    {
        _entityManager = new EntityManager(4096);
        _componentManager = new ComponentManager();
        _systems = new List<ISystem>();
    }

    public EntityId CreateEntity()
    {
        return _entityManager.CreateEntity();
    }

    public void DestroyEntity(EntityId entityId)
    {
        _componentManager.OnEntityDestroyed(entityId);
        _entityManager.DestroyEntity(entityId);
    }

    public void AddComponent<T>(EntityId entityId, T component) where T : struct
    {
        _componentManager.Add(entityId, component);
    }

    public T GetComponent<T>(EntityId entityId) where T : struct
    {
        return _componentManager.Get<T>(entityId);
    }

    public ref T GetComponentRef<T>(EntityId entityId) where T : struct
    {
        return ref _componentManager.GetRef<T>(entityId);
    }

    public void RemoveComponent<T>(EntityId entityId) where T : struct
    {
        _componentManager.Remove<T>(entityId);
    }

    public bool HasComponent<T>(EntityId entityId) where T : struct
    {
        return _componentManager.Has<T>(entityId);
    }

    public void RegisterSystem(ISystem system)
    {
        _systems.Add(system);
    }

    public void InitializeSystems()
    {
        foreach (var system in _systems)
        {
            system.Initialize();
        }
    }

    public void UpdateSystems(float delta)
    {
        foreach (var system in _systems.OrderBy(s => s.Phase))
        {
            system.Update(delta);
        }
    }

    public void ShutdownSystems()
    {
        foreach (var system in _systems.AsEnumerable().Reverse())
        {
            system.Shutdown();
        }
    }

    public IEnumerable<EntityId> QueryEntities<T1>() where T1 : struct
    {
        var pool = _componentManager.GetPool<T1>();
        if (pool == null)
        {
            return Enumerable.Empty<EntityId>();
        }

        return pool.GetAllEntityIds();
    }

    public IEnumerable<EntityId> QueryEntities<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        var pool1 = _componentManager.GetPool<T1>();
        if (pool1 == null)
        {
            return Enumerable.Empty<EntityId>();
        }

        var pool2 = _componentManager.GetPool<T2>();
        if (pool2 == null)
        {
            return Enumerable.Empty<EntityId>();
        }

        return pool1.GetAllEntityIds().Where(id => pool2.Has<T2>(id));
    }

    public IEnumerable<EntityId> QueryEntities<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var pool1 = _componentManager.GetPool<T1>();
        if (pool1 == null)
        {
            return Enumerable.Empty<EntityId>();
        }

        var pool2 = _componentManager.GetPool<T2>();
        if (pool2 == null)
        {
            return Enumerable.Empty<EntityId>();
        }

        var pool3 = _componentManager.GetPool<T3>();
        if (pool3 == null)
        {
            return Enumerable.Empty<EntityId>();
        }

        return pool1.GetAllEntityIds().Where(id =>
            pool2.Has<T2>(id) && pool3.Has<T3>(id));
    }

    public IQuery CreateQuery()
    {
        throw new NotImplementedException("请使用 QueryEntities<T> 泛型方法");
    }

    public IArchetype GetArchetype(params Type[] componentTypes)
    {
        throw new NotImplementedException();
    }
}

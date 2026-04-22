using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;

namespace Genesis.Terraria.Systems;

public sealed class DamageSystem : ISystem
{
    private readonly EcsWorld _world;

    public SystemPhase Phase => SystemPhase.PostUpdate;

    public DamageSystem(EcsWorld world)
    {
        _world = world;
    }

    public void Initialize()
    {
    }

    public void Update(float delta)
    {
        var entitiesToProcess = new List<EntityId>();

        foreach (var entity in _world.QueryEntities<Health, Damage>())
        {
            entitiesToProcess.Add(entity);
        }

        foreach (var entity in entitiesToProcess)
        {
            if (!_world.HasComponent<Health>(entity) || !_world.HasComponent<Damage>(entity))
            {
                continue;
            }

            ref var health = ref _world.GetComponentRef<Health>(entity);
            var damage = _world.GetComponent<Damage>(entity);

            health.Current -= damage.Value;

            _world.RemoveComponent<Damage>(entity);

            if (health.Current <= 0)
            {
                health.Current = 0;

                if (_world.HasComponent<EnemyTag>(entity))
                {
                    var enemyTag = _world.GetComponent<EnemyTag>(entity);
                    var pos = _world.GetComponent<TilePosition>(entity);
                    Console.WriteLine($"[Terraria] {EnemyType.GetName(enemyTag.EnemyType)} 在 ({pos.X}, {pos.Y}) 被击败！");
                    _world.DestroyEntity(entity);
                }
                else if (_world.HasComponent<PlayerTag>(entity))
                {
                    Console.WriteLine("[Terraria] 玩家死亡！按 R 重生...");
                }
            }
        }
    }

    public void Shutdown()
    {
    }
}

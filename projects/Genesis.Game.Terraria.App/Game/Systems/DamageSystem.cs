using Gnosis.ECS.System;
using ECSWorld = Gnosis.ECS.World.World;

namespace Genesis.Game.Terraria.Game.Systems;

public sealed class DamageSystem : IWorldSystem
{
    private ECSWorld _world = null!;

    public SystemPhase Phase => SystemPhase.Update;

    public void SetWorld(ECSWorld world) => _world = world;
    public void Initialize() { }
    public void Shutdown() { }

    public void Update(float delta)
    {
        var damaged = _world.CreateQuery()
            .All<Health>()
            .All<Damage>()
            .Build();

        var toDestroy = new List<Gnosis.Core.Entity.EntityId>();

        foreach (var entityId in damaged)
        {
            var health = _world.GetComponent<Health>(entityId);
            var damage = _world.GetComponent<Damage>(entityId);

            health.Current -= damage.Value;

            _world.RemoveComponent<Damage>(entityId);

            if (health.Current <= 0)
            {
                health.Current = 0;

                if (_world.HasComponent<EnemyTag>(entityId))
                {
                    var enemyTag = _world.GetComponent<EnemyTag>(entityId);
                    var pos = _world.GetComponent<TilePosition>(entityId);
                    var name = GetEnemyName(enemyTag.EnemyType);
                    Console.WriteLine($"[Terraria] {name} 在 ({pos.X}, {pos.Y}) 被击败！");
                    toDestroy.Add(entityId);
                }
                else if (_world.HasComponent<PlayerTag>(entityId))
                {
                    Console.WriteLine("[Terraria] 玩家死亡！按 R 重生...");
                }
            }

            _world.SetComponent(entityId, health);
        }

        foreach (var entityId in toDestroy)
        {
            _world.DestroyEntity(entityId);
        }
    }

    private static string GetEnemyName(int enemyType)
    {
        return enemyType switch
        {
            EnemyType.Slime => "史莱姆",
            EnemyType.Zombie => "僵尸",
            EnemyType.DemonEye => "恶魔眼",
            _ => "未知敌人"
        };
    }
}

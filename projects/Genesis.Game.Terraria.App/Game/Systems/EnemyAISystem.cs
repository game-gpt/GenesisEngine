using Gnosis.ECS.System;
using ECSWorld = Gnosis.ECS.World.World;

namespace Genesis.Game.Terraria.Game.Systems;

public sealed class EnemyAISystem : IWorldSystem
{
    private ECSWorld _world = null!;
    private readonly TileMap _tileMap;
    private float _aiTimer;

    public SystemPhase Phase => SystemPhase.Update;

    public EnemyAISystem(TileMap tileMap)
    {
        _tileMap = tileMap;
    }

    public void SetWorld(ECSWorld world) => _world = world;
    public void Initialize() { }
    public void Shutdown() { }

    public void Update(float delta)
    {
        _aiTimer += delta;
        if (_aiTimer < 0.5f)
        {
            return;
        }
        _aiTimer = 0;

        var players = _world.CreateQuery()
            .All<PlayerTag>()
            .All<TilePosition>()
            .Build();

        var enemies = _world.CreateQuery()
            .All<EnemyTag>()
            .All<TilePosition>()
            .All<Velocity>()
            .Build();

        var rng = new Random();

        foreach (var playerId in players)
        {
            var playerPos = _world.GetComponent<TilePosition>(playerId);

            foreach (var enemyId in enemies)
            {
                var enemyTag = _world.GetComponent<EnemyTag>(enemyId);
                var pos = _world.GetComponent<TilePosition>(enemyId);

                var dx = playerPos.X - pos.X;
                var dy = playerPos.Y - pos.Y;
                var distance = (float)Math.Sqrt(dx * dx + dy * dy);

                switch (enemyTag.AiState)
                {
                    case AiState.Idle:
                        if (distance < 15)
                        {
                            enemyTag.AiState = AiState.Chase;
                        }
                        else if (rng.NextDouble() < 0.3)
                        {
                            enemyTag.AiState = AiState.Wander;
                        }
                        break;

                    case AiState.Wander:
                        if (distance < 15)
                        {
                            enemyTag.AiState = AiState.Chase;
                        }
                        else
                        {
                            var moveDir = rng.Next(-1, 2);
                            var newX = pos.X + moveDir;
                            if (!_tileMap.IsSolid(newX, pos.Y))
                            {
                                pos.X = newX;
                            }
                            if (rng.NextDouble() < 0.2)
                            {
                                enemyTag.AiState = AiState.Idle;
                            }
                        }
                        break;

                    case AiState.Chase:
                        if (distance > 20)
                        {
                            enemyTag.AiState = AiState.Idle;
                        }
                        else if (distance < 2)
                        {
                            enemyTag.AiState = AiState.Attack;
                        }
                        else
                        {
                            var moveX = dx > 0 ? 1 : dx < 0 ? -1 : 0;
                            var newX = pos.X + moveX;
                            if (!_tileMap.IsSolid(newX, pos.Y))
                            {
                                pos.X = newX;
                            }
                            else if (!_tileMap.IsSolid(newX, pos.Y - 1))
                            {
                                pos.X = newX;
                                pos.Y -= 1;
                            }
                        }
                        break;

                    case AiState.Attack:
                        if (distance > 3)
                        {
                            enemyTag.AiState = AiState.Chase;
                        }
                        else if (rng.NextDouble() < 0.4)
                        {
                            if (_world.HasComponent<Health>(playerId))
                            {
                                var damage = GetEnemyAttackDamage(enemyTag.EnemyType);
                                _world.AddComponent(playerId, new Damage { Value = damage, Source = enemyTag.EnemyType });
                            }
                        }
                        break;
                }

                _world.SetComponent(enemyId, enemyTag);
                _world.SetComponent(enemyId, pos);
            }
        }
    }

    private static int GetEnemyAttackDamage(int enemyType)
    {
        return enemyType switch
        {
            EnemyType.Slime => 5,
            EnemyType.Zombie => 10,
            EnemyType.DemonEye => 8,
            _ => 5
        };
    }
}

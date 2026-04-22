using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;

namespace Genesis.Terraria.Systems;

public sealed class EnemyAISystem : ISystem
{
    private readonly EcsWorld _world;
    private readonly Random _rng;
    private EntityId? _playerEntity;
    private float _aiTimer;

    public SystemPhase Phase => SystemPhase.Update;

    public EnemyAISystem(EcsWorld world)
    {
        _world = world;
        _rng = new Random();
        _aiTimer = 0;
    }

    public void Initialize()
    {
        foreach (var entity in _world.QueryEntities<PlayerTag, TilePosition>())
        {
            _playerEntity = entity;
            break;
        }
    }

    public void Update(float delta)
    {
        _aiTimer += delta;

        if (_aiTimer < 0.5f)
        {
            return;
        }

        _aiTimer = 0;

        if (!_playerEntity.HasValue)
        {
            return;
        }

        var playerPos = _world.GetComponent<TilePosition>(_playerEntity.Value);

        var tileMapEntity = EntityId.Null;
        foreach (var entity in _world.QueryEntities<TileMapData>())
        {
            tileMapEntity = entity;
            break;
        }

        if (tileMapEntity.IsNull)
        {
            return;
        }

        var tileMap = _world.GetComponent<TileMapData>(tileMapEntity);

        foreach (var entity in _world.QueryEntities<EnemyTag, TilePosition, Velocity>())
        {
            ref var enemyTag = ref _world.GetComponentRef<EnemyTag>(entity);
            ref var pos = ref _world.GetComponentRef<TilePosition>(entity);

            var dx = playerPos.X - pos.X;
            var dy = playerPos.Y - pos.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            switch (enemyTag.AiState)
            {
                case AiState.Idle:
                    UpdateIdleState(entity, ref enemyTag, ref pos, distance, in tileMap);
                    break;

                case AiState.Wander:
                    UpdateWanderState(entity, ref enemyTag, ref pos, distance, in tileMap);
                    break;

                case AiState.Chase:
                    UpdateChaseState(entity, ref enemyTag, ref pos, dx, dy, distance, in tileMap);
                    break;

                case AiState.Attack:
                    UpdateAttackState(entity, ref enemyTag, ref pos, distance);
                    break;
            }
        }
    }

    private void UpdateIdleState(EntityId entity, ref EnemyTag tag, ref TilePosition pos,
        double distanceToPlayer, in TileMapData tileMap)
    {
        if (distanceToPlayer < 15)
        {
            tag.AiState = AiState.Chase;
            return;
        }

        if (_rng.NextDouble() < 0.3)
        {
            tag.AiState = AiState.Wander;
        }
    }

    private void UpdateWanderState(EntityId entity, ref EnemyTag tag, ref TilePosition pos,
        double distanceToPlayer, in TileMapData tileMap)
    {
        if (distanceToPlayer < 15)
        {
            tag.AiState = AiState.Chase;
            return;
        }

        var moveDir = _rng.Next(-1, 2);
        var newX = pos.X + moveDir;

        if (!tileMap.IsSolid(newX, pos.Y))
        {
            pos.X = newX;
        }

        if (_rng.NextDouble() < 0.2)
        {
            tag.AiState = AiState.Idle;
        }
    }

    private void UpdateChaseState(EntityId entity, ref EnemyTag tag, ref TilePosition pos,
        int dx, int dy, double distance, in TileMapData tileMap)
    {
        if (distance > 20)
        {
            tag.AiState = AiState.Idle;
            return;
        }

        if (distance < 2)
        {
            tag.AiState = AiState.Attack;
            return;
        }

        var moveX = dx > 0 ? 1 : dx < 0 ? -1 : 0;
        var newX = pos.X + moveX;

        if (!tileMap.IsSolid(newX, pos.Y))
        {
            pos.X = newX;
        }
        else if (!tileMap.IsSolid(newX, pos.Y - 1))
        {
            pos.X = newX;
            pos.Y -= 1;
        }

        var moveY = dy > 0 ? 1 : dy < 0 ? -1 : 0;
        if (moveY != 0)
        {
            var newY = pos.Y + moveY;
            if (!tileMap.IsSolid(pos.X, newY))
            {
                pos.Y = newY;
            }
        }
    }

    private void UpdateAttackState(EntityId entity, ref EnemyTag tag, ref TilePosition pos,
        double distanceToPlayer)
    {
        if (distanceToPlayer > 3)
        {
            tag.AiState = AiState.Chase;
            return;
        }

        if (_rng.NextDouble() < 0.4)
        {
            if (_playerEntity.HasValue && _world.HasComponent<Health>(_playerEntity.Value))
            {
                var damage = EnemyType.GetAttackDamage(tag.EnemyType);
                _world.AddComponent(_playerEntity.Value, new Damage(damage, tag.EnemyType));
            }
        }
    }

    public void Shutdown()
    {
    }
}

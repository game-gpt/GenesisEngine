using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;

namespace Genesis.Terraria.Systems;

public sealed class PhysicsSystem : ISystem
{
    private readonly EcsWorld _world;

    public SystemPhase Phase => SystemPhase.Update;

    public PhysicsSystem(EcsWorld world)
    {
        _world = world;
    }

    public void Initialize()
    {
    }

    public void Update(float delta)
    {
        ApplyGravity(delta);
        ApplyTileCollision();
    }

    private void ApplyGravity(float delta)
    {
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

        foreach (var entity in _world.QueryEntities<Velocity, TilePosition>())
        {
            ref var vel = ref _world.GetComponentRef<Velocity>(entity);
            ref var pos = ref _world.GetComponentRef<TilePosition>(entity);

            vel.Vy += 9.8f * delta;

            if (vel.Vy > 20.0f)
            {
                vel.Vy = 20.0f;
            }

            var newY = pos.Y + (int)Math.Round(vel.Vy * delta);

            if (newY > pos.Y)
            {
                for (var y = pos.Y + 1; y <= newY && y < tileMap.Height; y++)
                {
                    if (tileMap.IsSolid(pos.X, y))
                    {
                        vel.Vy = 0;
                        newY = y - 1;
                        break;
                    }
                }
            }

            pos.Y = Math.Clamp(newY, 0, tileMap.Height - 1);
        }
    }

    private void ApplyTileCollision()
    {
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

        foreach (var entity in _world.QueryEntities<TilePosition, Velocity>())
        {
            var pos = _world.GetComponent<TilePosition>(entity);

            if (tileMap.IsSolid(pos.X, pos.Y))
            {
                ref var posRef = ref _world.GetComponentRef<TilePosition>(entity);
                ref var velRef = ref _world.GetComponentRef<Velocity>(entity);

                for (var y = posRef.Y - 1; y >= 0; y--)
                {
                    if (!tileMap.IsSolid(posRef.X, y))
                    {
                        posRef.Y = y;
                        velRef.Vy = 0;
                        break;
                    }
                }
            }
        }
    }

    public void Shutdown()
    {
    }
}

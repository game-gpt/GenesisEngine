using Gnosis.ECS.System;
using ECSWorld = Gnosis.ECS.World.World;

namespace Genesis.Game.Terraria.Game.Systems;

public sealed class PhysicsSystem : IWorldSystem
{
    private ECSWorld _world = null!;
    private readonly TileMap _tileMap;

    public SystemPhase Phase => SystemPhase.Update;

    public PhysicsSystem(TileMap tileMap)
    {
        _tileMap = tileMap;
    }

    public void SetWorld(ECSWorld world) => _world = world;
    public void Initialize() { }
    public void Shutdown() { }

    public void Update(float delta)
    {
        var moving = _world.CreateQuery()
            .All<Velocity>()
            .All<TilePosition>()
            .Build();

        foreach (var entityId in moving)
        {
            var pos = _world.GetComponent<TilePosition>(entityId);
            var vel = _world.GetComponent<Velocity>(entityId);

            vel.Vy += 9.8f * delta;

            if (vel.Vy > 20.0f)
            {
                vel.Vy = 20.0f;
            }

            var newY = pos.Y + (int)Math.Round(vel.Vy * delta);

            if (newY > pos.Y)
            {
                for (var y = pos.Y + 1; y <= newY; y++)
                {
                    if (_tileMap.IsSolid(pos.X, y))
                    {
                        vel.Vy = 0;
                        newY = y - 1;
                        break;
                    }
                }
            }

            if (newY < 0)
            {
                newY = 0;
            }
            if (newY >= _tileMap.Height)
            {
                newY = _tileMap.Height - 1;
            }

            pos.Y = newY;

            if (_tileMap.IsSolid(pos.X, pos.Y))
            {
                for (var y = pos.Y - 1; y >= 0; y--)
                {
                    if (!_tileMap.IsSolid(pos.X, y))
                    {
                        pos.Y = y;
                        vel.Vy = 0;
                        break;
                    }
                }
            }

            _world.SetComponent(entityId, pos);
            _world.SetComponent(entityId, vel);
        }
    }
}

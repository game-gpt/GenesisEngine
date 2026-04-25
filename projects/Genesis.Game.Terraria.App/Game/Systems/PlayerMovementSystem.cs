using Gnosis.ECS.System;
using ECSWorld = Gnosis.ECS.World.World;
using Gnosis.Input.Device;
using Gnosis.Input.Simulate;

namespace Genesis.Game.Terraria.Game.Systems;

public sealed class PlayerMovementSystem : IWorldSystem
{
    private ECSWorld _world = null!;
    private readonly TileMap _tileMap;
    private readonly InputSystem _inputSystem;

    public SystemPhase Phase => SystemPhase.Update;

    public PlayerMovementSystem(TileMap tileMap, InputSystem inputSystem)
    {
        _tileMap = tileMap;
        _inputSystem = inputSystem;
    }

    public void SetWorld(ECSWorld world) => _world = world;
    public void Initialize() { }
    public void Shutdown() { }

    public void Update(float delta)
    {
        var keyboard = _inputSystem.Keyboard;
        if (keyboard == null)
        {
            return;
        }

        var mouse = _inputSystem.Mouse;

        var players = _world.CreateQuery()
            .All<PlayerTag>()
            .All<TilePosition>()
            .All<Velocity>()
            .Build();

        foreach (var entityId in players)
        {
            var pos = _world.GetComponent<TilePosition>(entityId);
            var vel = _world.GetComponent<Velocity>(entityId);

            vel.Vx = 0.0f;

            if (keyboard.GetKey(KeyCode.A) || keyboard.GetKey(KeyCode.Left))
            {
                vel.Vx = -5.0f;
            }
            if (keyboard.GetKey(KeyCode.D) || keyboard.GetKey(KeyCode.Right))
            {
                vel.Vx = 5.0f;
            }
            if (keyboard.GetKeyDown(KeyCode.Space) || keyboard.GetKeyDown(KeyCode.Up))
            {
                TryJump(ref pos, ref vel);
            }
            if (mouse != null && mouse.GetButtonDown(MouseButton.Left))
            {
                MineTile(ref pos, 0, 1);
            }

            var newX = pos.X + vel.Vx * delta;
            var newY = pos.Y + vel.Vy * delta;

            if (!_tileMap.IsSolid((int)newX, pos.Y))
            {
                pos.X = (int)newX;
            }
            if (!_tileMap.IsSolid(pos.X, (int)newY))
            {
                pos.Y = (int)newY;
            }

            _world.SetComponent(entityId, pos);
            _world.SetComponent(entityId, vel);
        }
    }

    private void TryJump(ref TilePosition pos, ref Velocity vel)
    {
        if (pos.Y <= 0)
        {
            return;
        }
        var onGround = _tileMap.IsSolid(pos.X, pos.Y + 1);
        if (!onGround)
        {
            return;
        }
        if (!_tileMap.IsSolid(pos.X, pos.Y - 1))
        {
            pos.Y -= 1;
            vel.Vy = -5.0f;
            if (!_tileMap.IsSolid(pos.X, pos.Y - 1))
            {
                pos.Y -= 1;
            }
        }
    }

    private void MineTile(ref TilePosition pos, int dx, int dy)
    {
        var targetX = pos.X + dx;
        var targetY = pos.Y + dy;
        var tile = _tileMap.GetTile(targetX, targetY);
        if (tile == Tile.Air || tile == Tile.Boundary)
        {
            return;
        }
        _tileMap.SetTile(targetX, targetY, Tile.Air);
    }
}

using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;

namespace Genesis.Terraria.Systems;

public sealed class PlayerMovementSystem : ISystem
{
    private readonly EcsWorld _world;
    private EntityId? _playerEntity;
    private EntityId? _tileMapEntity;

    public SystemPhase Phase => SystemPhase.Update;

    public PlayerMovementSystem(EcsWorld world)
    {
        _world = world;
    }

    public void Initialize()
    {
        foreach (var entity in _world.QueryEntities<PlayerTag, TilePosition, Velocity>())
        {
            _playerEntity = entity;
            break;
        }

        foreach (var entity in _world.QueryEntities<TileMapData>())
        {
            _tileMapEntity = entity;
            break;
        }
    }

    public void Update(float delta)
    {
        if (!_playerEntity.HasValue || !_tileMapEntity.HasValue)
        {
            return;
        }

        if (!Console.KeyAvailable)
        {
            return;
        }

        var key = Console.ReadKey(true);

        ref var pos = ref _world.GetComponentRef<TilePosition>(_playerEntity.Value);
        ref var vel = ref _world.GetComponentRef<Velocity>(_playerEntity.Value);
        ref var tileMap = ref _world.GetComponentRef<TileMapData>(_tileMapEntity.Value);

        var moveSpeed = 1;

        switch (key.Key)
        {
            case ConsoleKey.A:
            case ConsoleKey.LeftArrow:
                TryMove(ref pos, -moveSpeed, 0, in tileMap);
                break;

            case ConsoleKey.D:
            case ConsoleKey.RightArrow:
                TryMove(ref pos, moveSpeed, 0, in tileMap);
                break;

            case ConsoleKey.W:
            case ConsoleKey.UpArrow:
            case ConsoleKey.Spacebar:
                TryJump(ref pos, ref vel, in tileMap);
                break;

            case ConsoleKey.S:
            case ConsoleKey.DownArrow:
                MineTile(ref pos, ref tileMap, 0, 1);
                break;

            case ConsoleKey.J:
                MineTile(ref pos, ref tileMap, -1, 0);
                break;

            case ConsoleKey.K:
                MineTile(ref pos, ref tileMap, 1, 0);
                break;

            case ConsoleKey.I:
                MineTile(ref pos, ref tileMap, 0, -1);
                break;

            case ConsoleKey.E:
                AttackNearby(ref pos);
                break;

            case ConsoleKey.D1:
            case ConsoleKey.D2:
            case ConsoleKey.D3:
            case ConsoleKey.D4:
            case ConsoleKey.D5:
            case ConsoleKey.D6:
            case ConsoleKey.D7:
            case ConsoleKey.D8:
            case ConsoleKey.D9:
            case ConsoleKey.D0:
                SelectInventorySlot(key.Key);
                break;
        }
    }

    private static void TryMove(ref TilePosition pos, int dx, int dy, in TileMapData tileMap)
    {
        var newX = pos.X + dx;
        var newY = pos.Y + dy;

        if (!tileMap.IsSolid(newX, newY))
        {
            pos.X = newX;
            pos.Y = newY;
        }
    }

    private static void TryJump(ref TilePosition pos, ref Velocity vel, in TileMapData tileMap)
    {
        if (pos.Y <= 0)
        {
            return;
        }

        var onGround = tileMap.IsSolid(pos.X, pos.Y + 1);
        if (!onGround)
        {
            return;
        }

        if (!tileMap.IsSolid(pos.X, pos.Y - 1))
        {
            pos.Y -= 1;

            if (!tileMap.IsSolid(pos.X, pos.Y - 1))
            {
                pos.Y -= 1;
            }
        }
    }

    private void MineTile(ref TilePosition pos, ref TileMapData tileMap, int dx, int dy)
    {
        var targetX = pos.X + dx;
        var targetY = pos.Y + dy;

        var tile = tileMap.GetTile(targetX, targetY);
        if (tile == TileId.Air || tile == TileId.Boundary)
        {
            return;
        }

        tileMap.SetTile(targetX, targetY, TileId.Air);

        if (!_playerEntity.HasValue)
        {
            return;
        }

        if (!_world.HasComponent<Inventory>(_playerEntity.Value))
        {
            return;
        }

        ref var inventory = ref _world.GetComponentRef<Inventory>(_playerEntity.Value);
        AddItemToInventory(ref inventory, tile, 1);
    }

    private static void AddItemToInventory(ref Inventory inventory, int itemId, int count)
    {
        for (var i = 0; i < inventory.Slots; i++)
        {
            if (inventory.ItemIds[i] == itemId)
            {
                inventory.ItemCounts[i] += count;
                return;
            }
        }

        for (var i = 0; i < inventory.Slots; i++)
        {
            if (inventory.ItemIds[i] == ItemId.None)
            {
                inventory.ItemIds[i] = itemId;
                inventory.ItemCounts[i] = count;
                return;
            }
        }
    }

    private void SelectInventorySlot(ConsoleKey key)
    {
        if (!_playerEntity.HasValue)
        {
            return;
        }

        if (!_world.HasComponent<Inventory>(_playerEntity.Value))
        {
            return;
        }

        ref var inventory = ref _world.GetComponentRef<Inventory>(_playerEntity.Value);

        var slot = key switch
        {
            ConsoleKey.D1 => 0,
            ConsoleKey.D2 => 1,
            ConsoleKey.D3 => 2,
            ConsoleKey.D4 => 3,
            ConsoleKey.D5 => 4,
            ConsoleKey.D6 => 5,
            ConsoleKey.D7 => 6,
            ConsoleKey.D8 => 7,
            ConsoleKey.D9 => 8,
            ConsoleKey.D0 => 9,
            _ => -1
        };

        if (slot >= 0 && slot < inventory.Slots)
        {
            inventory.SelectedSlot = slot;
        }
    }

    private void AttackNearby(ref TilePosition playerPos)
    {
        if (!_playerEntity.HasValue)
        {
            return;
        }

        var inventory = _world.GetComponent<Inventory>(_playerEntity.Value);
        var selectedItem = inventory.ItemIds[inventory.SelectedSlot];
        var damage = ItemId.GetAttackDamage(selectedItem);

        foreach (var enemy in _world.QueryEntities<EnemyTag, TilePosition, Health>())
        {
            var enemyPos = _world.GetComponent<TilePosition>(enemy);
            var dx = Math.Abs(enemyPos.X - playerPos.X);
            var dy = Math.Abs(enemyPos.Y - playerPos.Y);

            if (dx <= 2 && dy <= 2)
            {
                _world.AddComponent(enemy, new Damage(damage, 0));
            }
        }
    }

    public void Shutdown()
    {
    }
}
